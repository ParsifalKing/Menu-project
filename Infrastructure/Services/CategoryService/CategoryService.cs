using System.Net;
using Domain.DTOs.CategoryDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.CategoryService;

public class CategoryService(ILogger<CategoryService> logger, DataContext context) : ICategoryService
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

    public async Task<Response<GetCategoryDto>> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            logger.LogInformation("Starting method GetCategoryByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Categories.Select(x => new GetCategoryDto()
            {
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).FirstOrDefaultAsync(x => x.Id == categoryId);

            if (existing is null)
            {
                logger.LogWarning("Could not find Category with Id:{Id},time:{DateTimeNow}", categoryId, DateTimeOffset.UtcNow);
                return new Response<GetCategoryDto>(HttpStatusCode.BadRequest, $"Not found Category by id:{categoryId}");
            }


            logger.LogInformation("Finished method GetCategoryByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetCategoryDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetCategoryDto>(HttpStatusCode.InternalServerError, e.Message);
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

            await context.Categories.AddAsync(newCategory);
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

            existing!.Name = updateCategory.Name;
            existing.Description = updateCategory.Description;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
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

