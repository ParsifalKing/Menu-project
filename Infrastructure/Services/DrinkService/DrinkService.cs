using System.Net;
using Domain.DTOs.DrinkDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.DrinkService;

public class DrinkService(ILogger<DrinkService> logger, IFileService fileService, DataContext context) : IDrinkService
{

    #region GetDrinksAsync

    public async Task<PagedResponse<List<GetDrinkDto>>> GetDrinksAsync(DrinkFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinksAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var drinks = context.Drinks.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                drinks = drinks.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            if (filter.Price != null)
                drinks = drinks.Where(x => x.Price <= filter.Price);

            var response = await drinks.Select(x => new GetDrinkDto()
            {
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Count = x.Count,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await drinks.CountAsync();

            logger.LogInformation("Finished method GetDrinksAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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

    public async Task<Response<GetDrinkDto>> GetDrinkByIdAsync(Guid drinkId)
    {
        try
        {
            logger.LogInformation("Starting method GetDrinkByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Drinks.Select(x => new GetDrinkDto()
            {
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Count = x.Count,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).FirstOrDefaultAsync(x => x.Id == drinkId);

            if (existing is null)
            {
                logger.LogWarning("Could not find Drink with Id:{Id},time:{DateTimeNow}", drinkId, DateTimeOffset.UtcNow);
                return new Response<GetDrinkDto>(HttpStatusCode.BadRequest, $"Not found Drink by id:{drinkId}");
            }

            logger.LogInformation("Finished method GetDrinkByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetDrinkDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetDrinkDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateDrinkAsync

    public async Task<Response<string>> CreateDrinkAsync(CreateDrinkDto createDrink)
    {
        try
        {
            logger.LogInformation("Starting method CreateDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            if (createDrink.Count < 0 || createDrink.Price < 0)
            {
                logger.LogWarning("Error 400, coming incorrect count:{DrinkCount} or price:{DrinkPrice} in time{DateTime}", createDrink.Count, createDrink.Price, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Error 400, coming incorrect count:{createDrink.Count} or price:{createDrink.Price}");
            }

            var newDrink = new Drink()
            {
                Name = createDrink.Name,
                Description = createDrink.Description,
                Price = createDrink.Price,
                Count = createDrink.Count,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createDrink.Photo != null)
                newDrink.PathPhoto = await fileService.CreateFile(createDrink.Photo);

            await context.Drinks.AddAsync(newDrink);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method UpdateDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Drinks.FirstOrDefaultAsync(x => x.Id == updateDrink.Id);
            if (existing == null)
            {
                logger.LogWarning("Drink not found by id:{Id},time:{Time}", updateDrink.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest, $"Not found Drink by id:{updateDrink.Id}");
            }

            existing!.Name = updateDrink.Name;
            existing.Description = updateDrink.Description;
            existing.Count = updateDrink.Count;
            existing.Price = updateDrink.Price;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateDrink.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateDrink.Photo);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
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
            logger.LogInformation("Starting method DeleteDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var drink = await context.Drinks.Where(x => x.Id == drinkId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteDrinkAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return drink == 0
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

