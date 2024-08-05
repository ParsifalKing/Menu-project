using System.Net;
using Domain.DTOs.DrinkDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Infrastructure.Services.DrinkIngredientService;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DrinkService;

public class DrinkService(ILogger<DrinkService> logger, IFileService fileService, DataContext context,
 ICheckIngredientsService checkDrinkIngredientsService, IDrinkIngredientService DrinkIngredientService) : IDrinkService
{

    #region GetDrinksAsync

    public async Task<PagedResponse<List<GetDrinkDto>>> GetDrinksAsync(DrinkFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinksAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            var drinks = context.Drinks.AsQueryable();
            var checkDrinks = await context.Drinks.ToListAsync();

            // Every time we must update the property AreAllIngredient!!!
            foreach (var drink in checkDrinks)
            {
                await checkDrinkIngredientsService.CheckDrinkIngredients(drink.Id);
            }

            if (filter.IngredientName != null)
            {
                // filter with ingredient name
                var query = (from d in drinks
                             join di in context.DrinkIngredient on d.Id equals di.DrinkId
                             join i in context.Ingredients on di.IngredientId equals i.Id
                             where i.Name == filter.IngredientName
                             select new
                             {
                                 Drink = d,
                             }).Select(x => x.Drink);
                drinks = query;
            }
            if (!string.IsNullOrEmpty(filter.Name))
                drinks = drinks.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            if (filter.AreAllIngredients != null)
                drinks = drinks.Where(x => x.AreAllIngredients == filter.AreAllIngredients);
            if (filter.Price != null)
                drinks = drinks.Where(x => x.Price <= filter.Price);
            if (filter.CookingTimeInMinutes != null)
                drinks = drinks.Where(x => x.CookingTimeInMinutes <= filter.CookingTimeInMinutes);


            var response = await drinks.Select(x => new GetDrinkDto()
            {
                Name = x.Name,
                Description = x.Description,
                CookingTimeInMinutes = x.CookingTimeInMinutes,
                AreAllIngredients = x.AreAllIngredients,
                Price = x.Price,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await drinks.CountAsync();

            logger.LogInformation("Finished method GetDrinksAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDrinkDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDrinkDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region GetDrinkByIdAsync

    public async Task<Response<GetDrinkWithAllIngredients>> GetDrinkByIdAsync(Guid drinkId)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinkByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var drinkIngredients = await (from d in context.Drinks
                                          where d.Id == drinkId
                                          join di in context.DrinkIngredient on d.Id equals di.DrinkId into DrinkIngGroup
                                          from di in DrinkIngGroup.DefaultIfEmpty()
                                          join i in context.Ingredients on di.IngredientId equals i.Id into ingGroup
                                          from i in ingGroup.DefaultIfEmpty()
                                          select new
                                          {
                                              Drink = new GetDrinkDto()
                                              {
                                                  Name = d.Name,
                                                  Description = d.Description,
                                                  CookingTimeInMinutes = d.CookingTimeInMinutes,
                                                  AreAllIngredients = d.AreAllIngredients,
                                                  Price = d.Price,
                                                  PathPhoto = d.PathPhoto,
                                                  CreatedAt = d.CreatedAt,
                                                  UpdatedAt = d.UpdatedAt,
                                                  Id = d.Id,
                                              },
                                              DrinkIngredients = i != null ? new GetIngredientDto()
                                              {
                                                  Description = i.Description,
                                                  Name = i.Name,
                                                  Count = i.Count,
                                                  IsInReserve = i.IsInReserve,
                                                  Price = i.Price,
                                                  PathPhoto = i.PathPhoto,
                                                  CreatedAt = i.CreatedAt,
                                                  UpdatedAt = i.UpdatedAt,
                                                  Id = i.Id,
                                              } : null,
                                          }).ToListAsync();

            if (drinkIngredients is null || !drinkIngredients.Any())
            {
                logger.LogWarning("Could not find Drink with Id:{Id},time:{DateTimeNow}", drinkId, DateTimeOffset.UtcNow);
                return new Response<GetDrinkWithAllIngredients>(HttpStatusCode.BadRequest, $"Not found Drink by id:{drinkId}");
            }

            var drinkWithIngredients = new GetDrinkWithAllIngredients()
            {
                Drink = drinkIngredients.First().Drink,
                DrinkIngredients = drinkIngredients.Where(x => x.DrinkIngredients != null)
                                                   .Select(x => x.DrinkIngredients).ToList(),
            };

            drinkWithIngredients.Drink.AreAllIngredients = await checkDrinkIngredientsService.CheckDrinkIngredients(drinkId);

            logger.LogInformation("Finished method GetDrinkByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDrinkWithAllIngredients>(drinkWithIngredients);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDrinkWithAllIngredients>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region CreateDrinkAsync

    public async Task<Response<string>> CreateDrinkAsync(CreateDrinkDto createDrink)
    {
        try
        {
            logger.LogInformation("Starting method CreateDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            if (createDrink.DrinkIngredients != null)
            {
                foreach (var DrinkIngredient in createDrink.DrinkIngredients)
                {
                    var existing = await context.DrinkIngredient.AnyAsync(x => x.IngredientId == DrinkIngredient.IngredientId && x.DrinkId == DrinkIngredient.DrinkId);
                    if (existing)
                    {
                        logger.LogWarning("Ups - error 400, this Drink with id - {DrinkId}, already has this Ingredient with id - {IngredientId}. {Time}",
                        DrinkIngredient.DrinkId, DrinkIngredient.IngredientId, DateTimeOffset.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this Drink with id - {DrinkIngredient.DrinkId}, already has this Ingredient with id - {DrinkIngredient.IngredientId}");
                    }
                    if (DrinkIngredient.Quantity <= 0)
                    {
                        logger.LogWarning("Error 400, quantity of ingredients for Drink cannot be negative. Time{DateTime}", DateTime.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for Drink cannot be negative: {DrinkIngredient.Quantity}");
                    }
                }
            }
            if (createDrink.CookingTimeInMinutes < 0 || createDrink.Price < 0)
            {
                logger.LogWarning("Error 400, coming incorrect input of Price or CookingTimeInMinutes. Time:{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Please, enter correct price and cooking time of Drink. They came negative.");
            }

            var newDrink = new Drink()
            {
                Name = createDrink.Name,
                Description = createDrink.Description,
                Price = createDrink.Price,
                CookingTimeInMinutes = createDrink.CookingTimeInMinutes,
                AreAllIngredients = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createDrink.Photo != null)
                newDrink.PathPhoto = await fileService.CreateFile(createDrink.Photo);

            var addedDrink = await context.Drinks.AddAsync(newDrink);
            await context.SaveChangesAsync();

            // Creating DrinkIngredient
            if (createDrink.DrinkIngredients != null)
            {
                foreach (var DrinkIngredient in createDrink.DrinkIngredients)
                {
                    DrinkIngredient.DrinkId = addedDrink.Entity.Id;
                    var res = await DrinkIngredientService.CreateDrinkIngredientAsync(DrinkIngredient);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of Drink (CreateDrinkIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of Drink (CreateDrinkIngredient)");
                }
            }

            logger.LogInformation("Finished method CreateDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Drink by Id:{newDrink.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateDrinkAsync

    public async Task<Response<string>> UpdateDrinkAsync(UpdateDrinkDto updateDrink)
    {
        try
        {
            logger.LogInformation("Starting method UpdateDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            if (updateDrink.DrinkIngredients != null)
            {
                foreach (var drinkIngredient in updateDrink.DrinkIngredients)
                {
                    var existingIngredient = await context.DrinkIngredient.AnyAsync(x => x.IngredientId == drinkIngredient.IngredientId && x.DrinkId == drinkIngredient.DrinkId);
                    if (existingIngredient)
                    {
                        logger.LogWarning("Ups - error 400, this Drink with id - {DrinkId}, already has this Ingredient with id - {IngredientId}. {Time}",
                        drinkIngredient.DrinkId, drinkIngredient.IngredientId, DateTimeOffset.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this Drink with id - {drinkIngredient.DrinkId}, already has this Ingredient with id - {drinkIngredient.IngredientId}");
                    }
                    if (drinkIngredient.Quantity <= 0)
                    {
                        logger.LogWarning("Error 400, quantity of ingredients for Drink cannot be negative. Time{DateTime}", DateTime.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for Drink cannot be negative: {drinkIngredient.Quantity}");
                    }
                }
            }
            if (updateDrink.CookingTimeInMinutes < 0 || updateDrink.Price < 0)
            {
                logger.LogWarning("Error 400, coming incorrect input of Price or CookingTimeInMinutes. Time:{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Please, enter correct price and cooking time of Drink. They came negative or 0.");
            }


            var existing = await context.Drinks.FirstOrDefaultAsync(x => x.Id == updateDrink.Id);
            if (existing == null)
            {
                logger.LogWarning("Drink not found by id:{Id},time:{Time}", updateDrink.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest, $"Not found Drink by id:{updateDrink.Id}");
            }

            var foundDrinkIngredients = await context.DrinkIngredient.Where(x => x.DrinkId == updateDrink.Id).ToListAsync();
            if (foundDrinkIngredients != null) context.DrinkIngredient.RemoveRange(foundDrinkIngredients);


            bool areAllIngredients = await checkDrinkIngredientsService.CheckDrinkIngredients(updateDrink.Id);
            existing!.AreAllIngredients = areAllIngredients;
            existing.Name = updateDrink.Name;
            existing.Description = updateDrink.Description;
            existing.CookingTimeInMinutes = updateDrink.CookingTimeInMinutes;
            existing.Price = updateDrink.Price;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateDrink.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateDrink.Photo);
            }
            await context.SaveChangesAsync();


            // Creating DrinkIngredient
            if (updateDrink.DrinkIngredients != null)
            {
                foreach (var drinkIngredient in updateDrink.DrinkIngredients)
                {
                    drinkIngredient.DrinkId = existing.Id;
                    var res = await DrinkIngredientService.CreateDrinkIngredientAsync(drinkIngredient);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of Drink (CreateDrinkIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of Drink (CreateDrinkIngredient)");
                }
            }

            logger.LogInformation("Finished method UpdateDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated Drink by id:{updateDrink.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteDrinkAsync

    public async Task<Response<bool>> DeleteDrinkAsync(Guid drinkId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var Drink = await context.Drinks.Where(x => x.Id == drinkId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDrinkAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return Drink == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"Drink not found by id:{drinkId}")
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

