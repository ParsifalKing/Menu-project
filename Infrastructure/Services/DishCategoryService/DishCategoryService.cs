using System.Net;
using Domain.DTOs.CategoryDTOs;
using Domain.DTOs.DishCategoryDTOs;
using Domain.DTOs.DishDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DishCategoryService;

public class DishCategoryService(ILogger<DishCategoryService> logger, DataContext context, ICheckIngredientsService checkDishIngredientsService) : IDishCategoryService
{
    #region GetDishCategoryAsync

    public async Task<PagedResponse<List<GetDishCategoryDto>>> GetDishCategoryAsync(PaginationFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var dishCategory = context.DishesCategories.AsQueryable();
            var checkDishes = await context.Dishes.ToListAsync();

            foreach (var item in checkDishes)
            {
                await checkDishIngredientsService.CheckDishIngredients(item.Id);
            }

            var response = await dishCategory.Include(x => x.Dish)
            .Include(x => x.Category)
            .Select(x => new GetDishCategoryDto()
            {
                Dish = new GetDishDto()
                {
                    Name = x.Dish!.Name,
                    Description = x.Dish!.Description,
                    CookingTimeInMinutes = x.Dish!.CookingTimeInMinutes,
                    Price = x.Dish!.Price,
                    AreAllIngredients = x.Dish!.AreAllIngredients,
                    Id = x.Dish!.Id,
                    Calorie = x.Dish!.Calorie,
                    PathPhoto = x.Dish!.PathPhoto,
                    CreatedAt = x.Dish!.CreatedAt,
                    UpdatedAt = x.Dish!.UpdatedAt,
                },
                Category = new GetCategoryDto()
                {
                    Name = x.Category!.Name,
                    Description = x.Category!.Description,
                    Id = x.Category!.Id,
                    CreatedAt = x.Category!.CreatedAt,
                    UpdatedAt = x.Category!.UpdatedAt,
                },
                CategoryId = x.CategoryId,
                DishId = x.DishId,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await dishCategory.CountAsync();

            logger.LogInformation("Finished method GetDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishCategoryDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishCategoryDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetDishCategoryByIdAsync

    public async Task<Response<GetDishCategoryDto>> GetDishCategoryByIdAsync(DishCategoryDto dishCategoryDto)
    {
        try
        {
            logger.LogInformation("Starting method GetDishCategoryByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.DishesCategories.Include(x => x.Dish)
            .Include(x => x.Category)
            .Select(x => new GetDishCategoryDto()
            {
                Dish = new GetDishDto()
                {
                    Name = x.Dish!.Name,
                    Description = x.Dish!.Description,
                    CookingTimeInMinutes = x.Dish!.CookingTimeInMinutes,
                    Price = x.Dish!.Price,
                    AreAllIngredients = x.Dish!.AreAllIngredients,
                    Id = x.Dish!.Id,
                    Calorie = x.Dish!.Calorie,
                    PathPhoto = x.Dish!.PathPhoto,
                    CreatedAt = x.Dish!.CreatedAt,
                    UpdatedAt = x.Dish!.UpdatedAt,
                },
                Category = new GetCategoryDto()
                {
                    Name = x.Category!.Name,
                    Description = x.Category!.Description,
                    Id = x.Category!.Id,
                    CreatedAt = x.Category!.CreatedAt,
                    UpdatedAt = x.Category!.UpdatedAt,
                },
                CategoryId = x.CategoryId,
                DishId = x.DishId,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).FirstOrDefaultAsync(x => x.CategoryId == dishCategoryDto.CategoryId && x.DishId == dishCategoryDto.DishId);

            if (existing is null)
            {
                logger.LogWarning("Could not find DishCategory with categoryId:{categoryId} and dishId:{dishId}, time:{DateTimeNow}",
                dishCategoryDto.CategoryId, dishCategoryDto.DishId, DateTimeOffset.UtcNow);
                return new Response<GetDishCategoryDto>(HttpStatusCode.BadRequest, $"Not found DishCategory with categoryId:{dishCategoryDto.CategoryId} and dishId:{dishCategoryDto.DishId}");
            }
            existing.Dish!.AreAllIngredients = await checkDishIngredientsService.CheckDishIngredients(existing.DishId);

            logger.LogInformation("Finished method GetDishCategoryByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDishCategoryDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDishCategoryDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateDishCategoryAsync

    public async Task<Response<string>> CreateDishCategoryAsync(DishCategoryDto createDishCategory)
    {
        try
        {
            logger.LogInformation("Starting method CreateDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var existing = await context.DishesCategories.AnyAsync(x => x.CategoryId == createDishCategory.CategoryId && x.DishId == createDishCategory.DishId);
            if (existing == true)
            {
                logger.LogWarning("Ups - error 400, this dish with id - {DishId}, already has this category with id - {CategoryId}. {Time}",
                createDishCategory.DishId, createDishCategory.CategoryId, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this dish with id - {createDishCategory.DishId}, already has this category with id - {createDishCategory.CategoryId}");
            }

            var newDishCategory = new DishCategory()
            {
                CategoryId = createDishCategory.CategoryId,
                DishId = createDishCategory.DishId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await context.DishesCategories.AddAsync(newDishCategory);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created DishCategory by Id:{newDishCategory.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateDishCategoryAsync

    public async Task<Response<string>> UpdateDishCategoryAsync(UpdateDishCategoryDto updateDishCategory)
    {
        try
        {
            logger.LogInformation("Starting method UpdateDishCategoryAsync in time:{DateTime}", DateTimeOffset.UtcNow);

            var existing = await context.DishesCategories.FirstOrDefaultAsync(x => x.Id == updateDishCategory.Id);
            if (existing == null)
            {
                logger.LogWarning("DishCategory not found by id:{Id}, time:{Time}",
                    updateDishCategory.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest,
                    $"Not found DishCategory by Id:{updateDishCategory.Id}");
            }

            existing!.DishId = updateDishCategory.DishId;
            existing.CategoryId = updateDishCategory.CategoryId;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated DishCategory by Id:{existing.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteDishCategoryAsync

    public async Task<Response<bool>> DeleteDishCategoryAsync(DishCategoryDto dishCategoryDto)
    {
        try
        {
            logger.LogInformation("Starting method DeleteDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var DishCategory = await context.DishesCategories.Where(x => x.CategoryId == dishCategoryDto.CategoryId && x.DishId == dishCategoryDto.DishId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDishCategoryAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return DishCategory == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"DishCategory not found by categoryId:{dishCategoryDto.CategoryId} and dishId:{dishCategoryDto.DishId}")
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

