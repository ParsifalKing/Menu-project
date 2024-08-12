using System.Net;
using Domain.DTOs.CategoryDTOs;
using Domain.DTOs.DishCategoryDTOs;
using Domain.DTOs.DishDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.DishCategoryService;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.CategoryService;

public class CategoryService(ILogger<CategoryService> logger, DataContext context, IFileService fileService, IDishCategoryService dishCategoryService) : ICategoryService
{

    #region GetCategoriesAsync

    public async Task<PagedResponse<List<GetCategoryDto>>> GetCategoriesAsync(CategoryFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetCategoriesAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var categories = context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                categories = categories.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

            var response = await categories.Select(x => new GetCategoryDto()
            {
                Name = x.Name,
                Description = x.Description,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            var totalRecord = await categories.CountAsync();

            logger.LogInformation("Finished method GetCategoriesAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetCategoryDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetCategoryDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetCategoryByIdAsync

    public async Task<Response<GetCategoryWithAllDishes>> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            logger.LogInformation("Starting method GetCategoryByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var categoryDishes = await (from c in context.Categories
                                        where c.Id == categoryId
                                        join dc in context.DishCategory on c.Id equals dc.CategoryId into CategoryIngGroup
                                        from dc in CategoryIngGroup.DefaultIfEmpty()
                                        join d in context.Dishes on dc.DishId equals d.Id into ingGroup
                                        from d in ingGroup.DefaultIfEmpty()
                                        select new
                                        {
                                            Category = new GetCategoryDto()
                                            {
                                                Name = c.Name,
                                                Description = c.Description,
                                                PathPhoto = c.PathPhoto,
                                                CreatedAt = c.CreatedAt,
                                                UpdatedAt = c.UpdatedAt,
                                                Id = c.Id,
                                            },
                                            CategoryIngredients = d != null ? new GetDishDto()
                                            {
                                                Description = d.Description,
                                                Name = d.Name,
                                                Calorie = d.Calorie,
                                                AreAllIngredients = d.AreAllIngredients,
                                                CookingTimeInMinutes = d.CookingTimeInMinutes,
                                                Price = d.Price,
                                                PathPhoto = d.PathPhoto,
                                                CreatedAt = d.CreatedAt,
                                                UpdatedAt = d.UpdatedAt,
                                                Id = d.Id,
                                            } : null,
                                        }).ToListAsync();

            if (categoryDishes is null || !categoryDishes.Any())
            {
                logger.LogWarning("Could not find Category with Id:{Id},time:{DateTimeNow}", categoryId, DateTimeOffset.UtcNow);
                return new Response<GetCategoryWithAllDishes>(HttpStatusCode.BadRequest, $"Not found Category by id:{categoryId}");
            }

            var categoryWithDishes = new GetCategoryWithAllDishes()
            {
                Category = categoryDishes.First().Category,
                CategoryDishes = categoryDishes.Where(x => x.CategoryIngredients != null)
                                                 .Select(x => x.CategoryIngredients).ToList(),
            };

            logger.LogInformation("Finished method GetCategoryByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetCategoryWithAllDishes>(categoryWithDishes);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetCategoryWithAllDishes>(HttpStatusCode.InternalServerError, e.Message);
        }
    }


    #endregion

    #region CreateCategoryAsync

    public async Task<Response<string>> CreateCategoryAsync(CreateCategoryDto createCategory)
    {
        try
        {
            logger.LogInformation("Starting method CreateCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Categories.AnyAsync(x => x.Name == createCategory.Name);
            if (existing == true) return new Response<string>(HttpStatusCode.BadRequest, $"This category by name:{createCategory.Name} already exist");

            var newCategory = new Category()
            {
                Name = createCategory.Name,
                Description = createCategory.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createCategory.Photo != null)
                newCategory.PathPhoto = await fileService.CreateFile(createCategory.Photo);

            var addedCategory = await context.Categories.AddAsync(newCategory);
            await context.SaveChangesAsync();

            // Creating CategoryDishes
            if (createCategory.CategoryDishes != null)
            {
                foreach (var categoryDishes in createCategory.CategoryDishes)
                {
                    var categoryDishesForDishCategory = new DishCategoryDto()
                    {
                        CategoryId = addedCategory.Entity.Id,
                        DishId = categoryDishes.DishId,
                    };
                    var res = await dishCategoryService.CreateDishCategoryAsync(categoryDishesForDishCategory);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }
            await context.SaveChangesAsync();


            logger.LogInformation("Finished method CreateCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Category by Id:{newCategory.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region UpdateCategoryAsync

    public async Task<Response<string>> UpdateCategoryAsync(UpdateCategoryDto updateCategory)
    {
        try
        {
            logger.LogInformation("Starting method UpdateCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Categories.FirstOrDefaultAsync(x => x.Id == updateCategory.Id);
            if (existing == null)
            {
                logger.LogWarning("Category not found by id:{Id},time:{Time}", updateCategory.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest, $"Not found Booking by id:{updateCategory.Id}");
            }

            var foundCategoryDishes = await context.DishCategory.Where(x => x.CategoryId == updateCategory.Id).ToListAsync();
            if (foundCategoryDishes != null) context.DishCategory.RemoveRange(foundCategoryDishes);

            existing!.Name = updateCategory.Name;
            existing.Description = updateCategory.Description;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateCategory.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateCategory.Photo);
            }
            await context.SaveChangesAsync();

            // Creating CategoryDishes
            if (updateCategory.CategoryDishes != null)
            {
                foreach (var dishIngredient in updateCategory.CategoryDishes)
                {
                    dishIngredient.CategoryId = existing.Id;
                    var res = await dishCategoryService.CreateDishCategoryAsync(dishIngredient);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }
            await context.SaveChangesAsync();


            logger.LogInformation("Finished method UpdateCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated Category by id:{updateCategory.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region DeleteCategoryAsync

    public async Task<Response<bool>> DeleteCategoryAsync(Guid categoryId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var category = await context.Categories.Where(x => x.Id == categoryId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return category == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"Category not found by id:{categoryId}")
                : new Response<bool>(true);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<bool>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

}

