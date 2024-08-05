using System.Net;
using Domain.DTOs.DrinkDTOs;
using Domain.DTOs.DrinkIngredientDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DrinkIngredientService;

public class DrinkIngredientService(ILogger<DrinkIngredientService> logger, DataContext context, ICheckIngredientsService checkIngredientsService) : IDrinkIngredientService
{

    #region GetDrinkIngredientAsync

    public async Task<PagedResponse<List<GetDrinkIngredientDto>>> GetDrinkIngredientAsync(PaginationFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var DrinkIngredient = context.DrinkIngredient.AsQueryable();
            var checkDrinkes = await context.Drinks.ToListAsync();

            foreach (var drink in checkDrinkes)
            {
                await checkIngredientsService.CheckDrinkIngredients(drink.Id);
            }

            var response = await DrinkIngredient.Include(x => x.Drink)
            .Include(x => x.Ingredient)
            .Select(x => new GetDrinkIngredientDto()
            {
                Drink = new GetDrinkDto()
                {
                    Name = x.Drink!.Name,
                    Description = x.Drink!.Description,
                    CookingTimeInMinutes = x.Drink!.CookingTimeInMinutes,
                    Price = x.Drink!.Price,
                    AreAllIngredients = x.Drink!.AreAllIngredients,
                    Id = x.Drink!.Id,
                    PathPhoto = x.Drink!.PathPhoto,
                    CreatedAt = x.Drink!.CreatedAt,
                    UpdatedAt = x.Drink!.UpdatedAt,
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
                DrinkId = x.DrinkId,
                Description = x.Description,
                Quantity = x.Quantity,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await DrinkIngredient.CountAsync();

            logger.LogInformation("Finished method GetDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDrinkIngredientDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDrinkIngredientDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetDrinkIngredientByIdAsync

    public async Task<Response<GetDrinkIngredientDto>> GetDrinkIngredientByIdAsync(Guid drinkId, Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinkIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.DrinkIngredient.Include(x => x.Drink)
            .Include(x => x.Ingredient)
            .Select(x => new GetDrinkIngredientDto()
            {
                Drink = new GetDrinkDto()
                {
                    Name = x.Drink!.Name,
                    Description = x.Drink!.Description,
                    CookingTimeInMinutes = x.Drink!.CookingTimeInMinutes,
                    Price = x.Drink!.Price,
                    AreAllIngredients = x.Drink!.AreAllIngredients,
                    Id = x.Drink!.Id,
                    PathPhoto = x.Drink!.PathPhoto,
                    CreatedAt = x.Drink!.CreatedAt,
                    UpdatedAt = x.Drink!.UpdatedAt,
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
                DrinkId = x.DrinkId,
                Description = x.Description,
                Quantity = x.Quantity,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).FirstOrDefaultAsync(x => x.IngredientId == ingredientId && x.DrinkId == drinkId);

            if (existing is null)
            {
                logger.LogWarning("Could not find DrinkIngredient with IngredientId:{IngredientId} and drinkId:{drinkId}, time:{DateTimeNow}",
                ingredientId, drinkId, DateTimeOffset.UtcNow);
                return new Response<GetDrinkIngredientDto>(HttpStatusCode.BadRequest, $"Not found DrinkIngredient with IngredientId:{ingredientId} and drinkId:{drinkId}");
            }
            existing.Drink!.AreAllIngredients = await checkIngredientsService.CheckDrinkIngredients(existing.DrinkId);

            logger.LogInformation("Finished method GetDrinkIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDrinkIngredientDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDrinkIngredientDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateDrinkIngredientAsync

    public async Task<Response<string>> CreateDrinkIngredientAsync(DrinkIngredientDto createDrinkIngredient)
    {
        try
        {
            logger.LogInformation("Starting method CreateDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var existing = await context.DrinkIngredient.AnyAsync(x => x.IngredientId == createDrinkIngredient.IngredientId && x.DrinkId == createDrinkIngredient.DrinkId);
            if (existing == true)
            {
                logger.LogWarning("Ups - error 400, this Drink with id - {drinkId}, already has this Ingredient with id - {IngredientId}. {Time}",
                createDrinkIngredient.DrinkId, createDrinkIngredient.IngredientId, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this Drink with id - {createDrinkIngredient.DrinkId}, already has this Ingredient with id - {createDrinkIngredient.IngredientId}");
            }
            if (createDrinkIngredient.Quantity <= 0)
            {
                logger.LogWarning("Error 400, quantity of ingredients for Drink cannot be negative. Time{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for Drink cannot be negative: {createDrinkIngredient.Quantity}");
            }

            var newDrinkIngredient = new DrinkIngredient()
            {
                IngredientId = createDrinkIngredient.IngredientId,
                DrinkId = createDrinkIngredient.DrinkId,
                Description = createDrinkIngredient.Description,
                Quantity = createDrinkIngredient.Quantity,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await context.DrinkIngredient.AddAsync(newDrinkIngredient);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created DrinkIngredient by Id:{newDrinkIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateDrinkIngredientAsync

    public async Task<Response<string>> UpdateDrinkIngredientAsync(UpdateDrinkIngredientDto updateDrinkIngredient)
    {
        try
        {
            logger.LogInformation("Starting method UpdateDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.DrinkIngredient.FirstOrDefaultAsync(x => x.Id == updateDrinkIngredient.Id);
            if (existing == null)
            {
                logger.LogWarning("DrinkIngredient not found by Id:{Id} , time:{Time}",
                    updateDrinkIngredient.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest,
                    $"Not found DrinkIngredient by Id:{updateDrinkIngredient.Id}");
            }
            if (updateDrinkIngredient.Quantity <= 0)
            {
                logger.LogWarning("Error 400, quantity of ingredients for Drink cannot be negative. Time{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for Drink cannot be negative: {updateDrinkIngredient.Quantity}");
            }

            existing!.DrinkId = updateDrinkIngredient.DrinkId;
            existing.IngredientId = updateDrinkIngredient.IngredientId;
            existing.Description = updateDrinkIngredient.Description;
            existing.Quantity = updateDrinkIngredient.Quantity;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated DrinkIngredient by Id:{updateDrinkIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteDrinkIngredientAsync

    public async Task<Response<bool>> DeleteDrinkIngredientAsync(Guid drinkId, Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var DrinkIngredient = await context.DrinkIngredient.Where(x => x.IngredientId == ingredientId && x.DrinkId == drinkId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDrinkIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return DrinkIngredient == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"DrinkIngredient not found by IngredientId:{ingredientId} and drinkId:{drinkId}")
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