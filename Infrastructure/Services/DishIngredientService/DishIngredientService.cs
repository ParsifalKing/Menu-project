using System.Net;
using Domain.DTOs.DishDTOs;
using Domain.DTOs.DishIngredientDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DishIngredientService;

public class DishIngredientService(ILogger<DishIngredientService> logger, DataContext context, ICheckIngredientsService checkDishIngredientsService) : IDishIngredientService
{

    #region GetDishIngredientAsync

    public async Task<PagedResponse<List<GetDishIngredientDto>>> GetDishIngredientAsync(PaginationFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var dishIngredient = context.DishesIngredients.AsQueryable();
            var checkDishes = await context.Dishes.ToListAsync();

            foreach (var dish in checkDishes)
            {
                await checkDishIngredientsService.CheckDishIngredients(dish.Id);
            }

            var response = await dishIngredient.Include(x => x.Dish)
            .Include(x => x.Ingredient)
            .Select(x => new GetDishIngredientDto()
            {
                Dish = new GetDishDto()
                {
                    Name = x.Dish!.Name,
                    Description = x.Dish!.Description,
                    CookingTimeInMinutes = x.Dish!.CookingTimeInMinutes,
                    Price = x.Dish!.Price,
                    AreAllIngredients = x.Dish!.AreAllIngredients,
                    Calorie = x.Dish!.Calorie,
                    Id = x.Dish!.Id,
                    PathPhoto = x.Dish!.PathPhoto,
                    CreatedAt = x.Dish!.CreatedAt,
                    UpdatedAt = x.Dish!.UpdatedAt,
                },
                Ingredient = new GetIngredientDto()
                {
                    Name = x.Ingredient!.Name,
                    Description = x.Ingredient!.Description,
                    Id = x.Ingredient!.Id,
                    Count = x.Ingredient!.Count,
                    IsInReserve = x.Ingredient!.IsInReserve,
                    PathPhoto = x.Ingredient!.PathPhoto,
                    Price = x.Ingredient!.Price,
                    CreatedAt = x.Ingredient!.CreatedAt,
                    UpdatedAt = x.Ingredient!.UpdatedAt,
                },
                IngredientId = x.IngredientId,
                DishId = x.DishId,
                Description = x.Description,
                Quantity = x.Quantity,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await dishIngredient.CountAsync();

            logger.LogInformation("Finished method GetDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishIngredientDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishIngredientDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetDishIngredientByIdAsync

    public async Task<Response<GetDishIngredientDto>> GetDishIngredientByIdAsync(Guid dishId, Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method GetDishIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.DishesIngredients.Include(x => x.Dish)
            .Include(x => x.Ingredient)
            .Select(x => new GetDishIngredientDto()
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
                Ingredient = new GetIngredientDto()
                {
                    Name = x.Ingredient!.Name,
                    Description = x.Ingredient!.Description,
                    Id = x.Ingredient!.Id,
                    Count = x.Ingredient!.Count,
                    IsInReserve = x.Ingredient!.IsInReserve,
                    PathPhoto = x.Ingredient!.PathPhoto,
                    Price = x.Ingredient!.Price,
                    CreatedAt = x.Ingredient!.CreatedAt,
                    UpdatedAt = x.Ingredient!.UpdatedAt,
                },
                IngredientId = x.IngredientId,
                DishId = x.DishId,
                Description = x.Description,
                Quantity = x.Quantity,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).FirstOrDefaultAsync(x => x.IngredientId == ingredientId && x.DishId == dishId);

            if (existing is null)
            {
                logger.LogWarning("Could not find DishIngredient with IngredientId:{IngredientId} and dishId:{dishId}, time:{DateTimeNow}",
                ingredientId, dishId, DateTimeOffset.UtcNow);
                return new Response<GetDishIngredientDto>(HttpStatusCode.BadRequest, $"Not found DishIngredient with IngredientId:{ingredientId} and dishId:{dishId}");
            }
            existing.Dish!.AreAllIngredients = await checkDishIngredientsService.CheckDishIngredients(existing.DishId);

            logger.LogInformation("Finished method GetDishIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDishIngredientDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDishIngredientDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateDishIngredientAsync

    public async Task<Response<string>> CreateDishIngredientAsync(DishIngredientDto createDishIngredient)
    {
        try
        {
            logger.LogInformation("Starting method CreateDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var existing = await context.DishesIngredients.AnyAsync(x => x.IngredientId == createDishIngredient.IngredientId && x.DishId == createDishIngredient.DishId);
            if (existing == true)
            {
                logger.LogWarning("Ups - error 400, this dish with id - {DishId}, already has this Ingredient with id - {IngredientId}. {Time}",
                createDishIngredient.DishId, createDishIngredient.IngredientId, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this dish with id - {createDishIngredient.DishId}, already has this Ingredient with id - {createDishIngredient.IngredientId}");
            }
            if (createDishIngredient.Quantity <= 0)
            {
                logger.LogWarning("Error 400, quantity of ingredients for dish cannot be negative. Time{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for dish cannot be negative: {createDishIngredient.Quantity}");
            }

            var newDishIngredient = new DishIngredient()
            {
                IngredientId = createDishIngredient.IngredientId,
                DishId = createDishIngredient.DishId,
                Description = createDishIngredient.Description,
                Quantity = createDishIngredient.Quantity,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await context.DishesIngredients.AddAsync(newDishIngredient);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created DishIngredient by Id:{newDishIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateDishIngredientAsync

    public async Task<Response<string>> UpdateDishIngredientAsync(UpdateDishIngredientDto updateDishIngredient)
    {
        try
        {
            logger.LogInformation("Starting method UpdateDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.DishesIngredients.FirstOrDefaultAsync(x => x.Id == updateDishIngredient.Id);
            if (existing == null)
            {
                logger.LogWarning("DishIngredient not found by Id:{Id} , time:{Time}",
                    updateDishIngredient.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest,
                    $"Not found DishIngredient by Id:{updateDishIngredient.Id}");
            }
            if (updateDishIngredient.Quantity <= 0)
            {
                logger.LogWarning("Error 400, quantity of ingredients for dish cannot be negative. Time{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for dish cannot be negative: {updateDishIngredient.Quantity}");
            }

            existing!.DishId = updateDishIngredient.DishId;
            existing.IngredientId = updateDishIngredient.IngredientId;
            existing.Description = updateDishIngredient.Description;
            existing.Quantity = updateDishIngredient.Quantity;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated DishIngredient by Id:{updateDishIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteDishIngredientAsync

    public async Task<Response<bool>> DeleteDishIngredientAsync(Guid dishId, Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var dishIngredient = await context.DishesIngredients.Where(x => x.IngredientId == ingredientId && x.DishId == dishId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDishIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return dishIngredient == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"DishIngredient not found by IngredientId:{ingredientId} and dishId:{dishId}")
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