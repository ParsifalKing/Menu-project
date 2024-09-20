using System.Net;
using Domain.DTOs.DishCategoryDTOs;
using Domain.DTOs.DishDTOs;
using Domain.DTOs.DishIngredientDTOs;
using Domain.DTOs.DrinkDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Infrastructure.Services.DishCategoryService;
using Infrastructure.Services.DishIngredientService;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DishService;

public class DishService(ILogger<DishService> logger, IFileService fileService, DataContext context,
 ICheckIngredientsService checkDishIngredientsService, IDishIngredientService dishIngredientService,
  IDishCategoryService dishCategoryService) : IDishService
{

    #region GetDishesWithDrinks

    public async Task<Response<List<Object>>> GetDishesWithDrinks()
    {
        try
        {
            logger.LogInformation("Starting method GetDishesWithDrinks in time:{DateTime} ", DateTimeOffset.UtcNow);

            var dishesAndDrinks = new List<Object>();
            // Get 6 random dishes
            var randomDishes = await context.Dishes
            .Select(x => new GetDishDto()
            {
                Calorie = x.Calorie,
                Description = x.Description,
                Name = x.Name,
                AreAllIngredients = x.AreAllIngredients,
                CookingTimeInMinutes = x.CookingTimeInMinutes,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Id = x.Id,
                Price = x.Price,
                PathPhoto = x.PathPhoto,
            })
            .OrderBy(d => Guid.NewGuid())
            .Take(6)
            .ToListAsync();

            // Get 3 random drinks
            var randomDrinks = await context.Drinks
            .Select(x => new GetDrinkDto()
            {
                Description = x.Description,
                Name = x.Name,
                AreAllIngredients = x.AreAllIngredients,
                CookingTimeInMinutes = x.CookingTimeInMinutes,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Id = x.Id,
                Price = x.Price,
                PathPhoto = x.PathPhoto,
            })
            .OrderBy(dr => Guid.NewGuid())
            .Take(3)
            .ToListAsync();

            dishesAndDrinks.AddRange(randomDishes);
            dishesAndDrinks.AddRange(randomDrinks);

            logger.LogInformation("Finished method GetDishesWithDrinks in time:{DateTime}", DateTimeOffset.UtcNow);
            return new Response<List<object>>(dishesAndDrinks);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<List<Object>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region GetDishesAsync

    public async Task<PagedResponse<List<GetDishDto>>> GetDishesAsync(DishFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDishesAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            var dishes = context.Dishes.AsQueryable();
            var checkDishes = await context.Dishes.ToListAsync();

            // Every time we must update the property AreAllIngredient!!!
            foreach (var dish in checkDishes)
            {
                await checkDishIngredientsService.CheckDishIngredients(dish.Id);
            }

            if (filter.IngredientName != null)
            {
                // filter with ingredient name
                var query = (from d in dishes
                             join di in context.DishesIngredients on d.Id equals di.DishId
                             join i in context.Ingredients on di.IngredientId equals i.Id
                             where i.Name == filter.IngredientName
                             select new
                             {
                                 Dish = d,
                             }).Select(x => x.Dish);
                dishes = query;
            }
            if (!string.IsNullOrEmpty(filter.Name))
                dishes = dishes.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            if (filter.AreAllIngredients != null)
                dishes = dishes.Where(x => x.AreAllIngredients == filter.AreAllIngredients);
            if (filter.Price != null)
                dishes = dishes.Where(x => x.Price <= filter.Price);
            if (filter.CookingTimeInMinutes != null)
                dishes = dishes.Where(x => x.CookingTimeInMinutes <= filter.CookingTimeInMinutes);
            if (filter.Calorie != null)
                dishes = dishes.Where(x => x.Calorie <= filter.Calorie);

            var response = await dishes.Select(x => new GetDishDto()
            {
                Name = x.Name,
                Description = x.Description,
                CookingTimeInMinutes = x.CookingTimeInMinutes,
                AreAllIngredients = x.AreAllIngredients,
                Price = x.Price,
                Calorie = x.Calorie,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await dishes.CountAsync();

            logger.LogInformation("Finished method GetDishesAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetDishDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region GetDishByIdAsync

    public async Task<Response<GetDishWithAllIngredients>> GetDishByIdAsync(Guid dishId)
    {
        try
        {
            logger.LogInformation("Starting method GetDishByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var dishIngredients = await (from d in context.Dishes
                                         where d.Id == dishId
                                         join di in context.DishesIngredients on d.Id equals di.DishId into dishIngGroup
                                         from di in dishIngGroup.DefaultIfEmpty()
                                         join i in context.Ingredients on di.IngredientId equals i.Id into ingGroup
                                         from i in ingGroup.DefaultIfEmpty()
                                         select new
                                         {
                                             Dish = new GetDishDto()
                                             {
                                                 Name = d.Name,
                                                 Description = d.Description,
                                                 CookingTimeInMinutes = d.CookingTimeInMinutes,
                                                 AreAllIngredients = d.AreAllIngredients,
                                                 Price = d.Price,
                                                 Calorie = d.Calorie,
                                                 PathPhoto = d.PathPhoto,
                                                 CreatedAt = d.CreatedAt,
                                                 UpdatedAt = d.UpdatedAt,
                                                 Id = d.Id,
                                             },
                                             DishIngredients = i != null ? new GetIngredientDto()
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

            if (!dishIngredients.Any())
            {
                logger.LogWarning("Could not find Dish with Id:{Id},time:{DateTimeNow}", dishId, DateTimeOffset.UtcNow);
                return new Response<GetDishWithAllIngredients>(HttpStatusCode.BadRequest, $"Not found Dish by id:{dishId}");
            }

            var dishWithIngredients = new GetDishWithAllIngredients()
            {
                Dish = dishIngredients.First().Dish,
                DishIngredients = dishIngredients.Where(x => x.DishIngredients != null)
                                                 .Select(x => x.DishIngredients).ToList(),
            };

            dishWithIngredients.Dish.AreAllIngredients = await checkDishIngredientsService.CheckDishIngredients(dishId);

            logger.LogInformation("Finished method GetDishByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDishWithAllIngredients>(dishWithIngredients);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDishWithAllIngredients>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region CreateDishAsync

    public async Task<Response<string>> CreateDishAsync(CreateDishDto createDish)
    {
        try
        {
            logger.LogInformation("Starting method CreateDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            if (createDish.DishIngredients != null)
            {
                foreach (var dishIngredient in createDish.DishIngredients)
                {
                    if (dishIngredient.Quantity <= 0)
                    {
                        logger.LogWarning("Error 400, quantity of ingredients for dish cannot be negative. Time{DateTime}", DateTime.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for dish cannot be negative: {dishIngredient.Quantity}");
                    }
                }
            }
            if (createDish.CookingTimeInMinutes <= 0 || createDish.Price <= 0)
            {
                logger.LogWarning("Error 400, coming incorrect input of Price or CookingTimeInMinutes. Time:{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Please, enter correct price and cooking time of dish. They came negative or 0.");
            }

            var newDish = new Dish()
            {
                Name = createDish.Name,
                Description = createDish.Description,
                Price = createDish.Price,
                Calorie = createDish.Calorie,
                CookingTimeInMinutes = createDish.CookingTimeInMinutes,
                AreAllIngredients = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createDish.Photo != null)
                newDish.PathPhoto = await fileService.CreateFile(createDish.Photo);

            var addedDish = await context.Dishes.AddAsync(newDish);
            await context.SaveChangesAsync();

            // Creating DishIngredient
            if (createDish.DishIngredients != null)
            {
                foreach (var dishIngredient in createDish.DishIngredients)
                {
                    var dishIngredientForCreateDish = new DishIngredientDto()
                    {
                        DishId = addedDish.Entity.Id,
                        IngredientId = dishIngredient.IngredientId,
                        Quantity = dishIngredient.Quantity,
                        Description = dishIngredient.Description,
                    };
                    var res = await dishIngredientService.CreateDishIngredientAsync(dishIngredientForCreateDish);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }


            // Creating DishCategory
            if (createDish.DishCategories != null)
            {
                foreach (var dishCategory in createDish.DishCategories)
                {
                    var dishCategoryForCreateDish = new DishCategoryDto()
                    {
                        DishId = addedDish.Entity.Id,
                        CategoryId = dishCategory.CategoryId,
                    };
                    var res = await dishCategoryService.CreateDishCategoryAsync(dishCategoryForCreateDish);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Dish by Id:{newDish.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region UpdateDishAsync

    public async Task<Response<string>> UpdateDishAsync(UpdateDishDto updateDish)
    {
        try
        {
            logger.LogInformation("Starting method UpdateDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            if (updateDish.DishIngredients != null)
            {
                foreach (var dishIngredient in updateDish.DishIngredients)
                {
                    var existingIngredient = await context.DishesIngredients.AnyAsync(x => x.IngredientId == dishIngredient.IngredientId && x.DishId == dishIngredient.DishId);
                    if (existingIngredient)
                    {
                        logger.LogWarning("Ups - error 400, this dish with id - {DishId}, already has this Ingredient with id - {IngredientId}. {Time}",
                        dishIngredient.DishId, dishIngredient.IngredientId, DateTimeOffset.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Error 400 , this dish with id - {dishIngredient.DishId}, already has this Ingredient with id - {dishIngredient.IngredientId}");
                    }
                    if (dishIngredient.Quantity <= 0)
                    {
                        logger.LogWarning("Error 400, quantity of ingredients for dish cannot be negative. Time{DateTime}", DateTime.UtcNow);
                        return new Response<string>(HttpStatusCode.BadRequest, $"Quantity of ingredients for dish cannot be negative: {dishIngredient.Quantity}");
                    }
                }
            }
            if (updateDish.CookingTimeInMinutes <= 0 || updateDish.Price <= 0)
            {
                logger.LogWarning("Error 400, coming incorrect input of Price or CookingTimeInMinutes. Time:{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Please, enter correct price and cooking time of dish. They came negative or 0.");
            }

            var existing = await context.Dishes.FirstOrDefaultAsync(x => x.Id == updateDish.Id);
            if (existing == null)
            {
                logger.LogWarning("Dish not found by id:{Id},time:{Time}", updateDish.Id, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not found Dish by id:{updateDish.Id}");
            }

            var foundDishIngredients = await context.DishesIngredients.Where(x => x.DishId == updateDish.Id).ToListAsync();
            if (foundDishIngredients.Any()) context.DishesIngredients.RemoveRange(foundDishIngredients);

            bool areAllIngredients = await checkDishIngredientsService.CheckDishIngredients(updateDish.Id);
            existing.AreAllIngredients = areAllIngredients;
            existing.Name = updateDish.Name;
            existing.Description = updateDish.Description;
            existing.CookingTimeInMinutes = updateDish.CookingTimeInMinutes;
            existing.Price = updateDish.Price;
            existing.Calorie = updateDish.Calorie;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateDish.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateDish.Photo);
            }
            await context.SaveChangesAsync();


            // Creating DishIngredient
            if (updateDish.DishIngredients != null)
            {
                foreach (var dishIngredient in updateDish.DishIngredients)
                {
                    dishIngredient.DishId = updateDish.Id;
                    var res = await dishIngredientService.CreateDishIngredientAsync(dishIngredient);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }


            // Creating DishCategory
            if (updateDish.DishCategories != null)
            {
                foreach (var dishCategory in updateDish.DishCategories)
                {
                    dishCategory.DishId = updateDish.Id;
                    var res = await dishCategoryService.CreateDishCategoryAsync(dishCategory);
                    if (res.StatusCode >= 400 && res.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 while saving ingredient of dish (CreateDishIngredient)");
                    if (res.StatusCode >= 500 && res.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 while saving ingredient of dish (CreateDishIngredient)");
                }
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated Dish by id:{updateDish.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region DeleteDishAsync

    public async Task<Response<bool>> DeleteDishAsync(Guid dishId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var dish = await context.Dishes.Where(x => x.Id == dishId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDishAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return dish == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"Dish not found by id:{dishId}")
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

