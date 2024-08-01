using System.Net;
using Domain.DTOs.DishDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckDishIngredientsService;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DishService;

public class DishService(ILogger<DishService> logger, IFileService fileService, DataContext context, ICheckDishIngredientsService checkDishIngredientsService) : IDishService
{

    #region GetDishesAsync

    public async Task<PagedResponse<List<GetDishDto>>> GetDishesAsync(DishFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDishesAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
                             join di in context.DishIngredient on d.Id equals di.DishId
                             join i in context.Ingredients on di.IngredientId equals i.Id
                             where i.Name == filter.IngredientName
                             select new
                             {
                                 Dish = d,
                             }).Select(x => x.Dish);
                dishes = query;
            }
            if (!string.IsNullOrEmpty(filter.CategoryName))
                dishes = dishes.Where(x => x.Name.ToLower().Contains(filter.CategoryName.ToLower()));
            if (filter.AreAllIngredients != null)
                dishes = dishes.Where(x => x.AreAllIngredients == filter.AreAllIngredients);
            if (filter.Price != null)
                dishes = dishes.Where(x => x.Price <= filter.Price);

            var response = await dishes.Select(x => new GetDishDto()
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
            var totalRecord = await dishes.CountAsync();

            logger.LogInformation("Finished method GetDishesAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method GetDishByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var dishIngredients = await (from d in context.Dishes
                                         join di in context.DishIngredient on d.Id equals di.DishId
                                         join i in context.Ingredients on di.IngredientId equals i.Id
                                         where d.Id == dishId
                                         select new
                                         {
                                             Dish = new GetDishDto()
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
                                             DishIngredients = new GetIngredientDto()
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
                                             },
                                         }).ToListAsync();

            if (dishIngredients is null)
            {
                logger.LogWarning("Could not find Dish with Id:{Id},time:{DateTimeNow}", dishId, DateTimeOffset.UtcNow);
                return new Response<GetDishWithAllIngredients>(HttpStatusCode.BadRequest, $"Not found Dish by id:{dishId}");
            }

            var dishWithIngredients = new GetDishWithAllIngredients()
            {
                Dish = dishIngredients.First().Dish,
                DishIngredients = dishIngredients.Select(x => x.DishIngredients).ToList(),
            };

            dishWithIngredients.Dish.AreAllIngredients = await checkDishIngredientsService.CheckDishIngredients(dishId);

            logger.LogInformation("Finished method GetDishByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method CreateDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var newDish = new Dish()
            {
                Name = createDish.Name,
                Description = createDish.Description,
                Price = createDish.Price,
                CookingTimeInMinutes = createDish.CookingTimeInMinutes,
                AreAllIngredients = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createDish.Photo != null)
                newDish.PathPhoto = await fileService.CreateFile(createDish.Photo);

            await context.Dishes.AddAsync(newDish);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method UpdateDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Dishes.FirstOrDefaultAsync(x => x.Id == updateDish.Id);
            if (existing == null)
            {
                logger.LogWarning("Dish not found by id:{Id},time:{Time}", updateDish.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest, $"Not found Dish by id:{updateDish.Id}");
            }

            bool areAllIngredients = await checkDishIngredientsService.CheckDishIngredients(updateDish.Id);
            existing!.AreAllIngredients = areAllIngredients;
            existing.Name = updateDish.Name;
            existing.Description = updateDish.Description;
            existing.CookingTimeInMinutes = updateDish.CookingTimeInMinutes;
            existing.Price = updateDish.Price;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateDish.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateDish.Photo);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method DeleteDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var dish = await context.Dishes.Where(x => x.Id == dishId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDishAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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

