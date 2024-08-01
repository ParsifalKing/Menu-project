using System.Net;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.IngredientService;

public class IngredientService(ILogger<IngredientService> logger, IFileService fileService, DataContext context) : IIngredientService
{

    #region GetIngredientsAsync

    public async Task<PagedResponse<List<GetIngredientDto>>> GetIngredientsAsync(IngredientFIlter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetIngredientsAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var ingredients = context.Ingredients.AsQueryable();

            foreach (var item in ingredients)
            {
                if (item.Count > 2) item.IsInReserve = true;
                else item.IsInReserve = false;
            }

            if (!string.IsNullOrEmpty(filter.Name))
                ingredients = ingredients.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            if (filter.Price != null)
                ingredients = ingredients.Where(x => x.Price <= filter.Price);
            if (filter.Count != null)
                ingredients = ingredients.Where(x => x.Count <= filter.Count);
            if (filter.IsInReserve != null)
                ingredients = ingredients.Where(x => x.IsInReserve == filter.IsInReserve);

            var response = await ingredients.Select(x => new GetIngredientDto()
            {
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Count = x.Count,
                IsInReserve = x.IsInReserve,
                PathPhoto = x.PathPhoto,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await ingredients.CountAsync();

            logger.LogInformation("Finished method GetIngredientsAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetIngredientDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetIngredientDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetIngredientByIdAsync

    public async Task<Response<GetIngredientDto>> GetIngredientByIdAsync(Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method GetIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Ingredients.FirstOrDefaultAsync(x => x.Id == ingredientId);
            if (existing is null)
            {
                logger.LogWarning("Could not find Ingredient with Id:{Id},time:{DateTimeNow}", ingredientId, DateTimeOffset.UtcNow);
                return new Response<GetIngredientDto>(HttpStatusCode.BadRequest, $"Not found Ingredient by id:{ingredientId}");
            }

            if (existing.Count > 2) existing.IsInReserve = true;
            else existing.IsInReserve = false;

            var ingredient = new GetIngredientDto
            {
                Name = existing.Name,
                Description = existing.Description,
                Price = existing.Price,
                IsInReserve = existing.IsInReserve,
                Count = existing.Count,
                PathPhoto = existing.PathPhoto,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt,
                Id = existing.Id,
            };
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method GetIngredientByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetIngredientDto>(ingredient);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetIngredientDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateIngredientAsync

    public async Task<Response<string>> CreateIngredientAsync(CreateIngredientDto createIngredient)
    {
        try
        {
            logger.LogInformation("Starting method CreateIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var existing = await context.Ingredients.AnyAsync(x => x.Name == createIngredient.Name && x.Description == createIngredient.Description);
            if (existing == true)
            {
                logger.LogInformation("This ingredient with name:{IngredientName} and decsription:{IngredientDescription} already exist. Error in time:{DateTime}",
                                    createIngredient.Name, createIngredient.Description, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.InternalServerError, $"This ingredient with name:{createIngredient.Name} and decsription:{createIngredient.Description} already exist");
            }
            if (createIngredient.Price < 0 || createIngredient.Count < 0)
            {
                logger.LogInformation("Error 400, incorrect input of Ingredient count{IngredientCount} or price{IngredientPrice} . Error in time:{DateTime}",
                                    createIngredient.Count, createIngredient.Price, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.InternalServerError, $"Error 400, incorrect input of Ingredient count:{createIngredient.Count} or price:{createIngredient.Price} ");
            }

            var newIngredient = new Ingredient()
            {
                Name = createIngredient.Name,
                Description = createIngredient.Description,
                Price = createIngredient.Price,
                Count = createIngredient.Count,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            if (createIngredient.Photo != null)
                newIngredient.PathPhoto = await fileService.CreateFile(createIngredient.Photo);
            if (createIngredient.Count > 2) newIngredient.IsInReserve = true;
            else newIngredient.IsInReserve = false;

            await context.Ingredients.AddAsync(newIngredient);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Ingredient by Id:{newIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateIngredientAsync

    public async Task<Response<string>> UpdateIngredientAsync(UpdateIngredientDto updateIngredient)
    {
        try
        {
            logger.LogInformation("Starting method UpdateIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var existing = await context.Ingredients.FirstOrDefaultAsync(x => x.Id == updateIngredient.Id);
            if (existing == null)
            {
                logger.LogWarning("Ingredient not found by id:{Id},time:{Time}", updateIngredient.Id, DateTimeOffset.UtcNow);
                new Response<string>(HttpStatusCode.BadRequest, $"Not found Ingredient by id:{updateIngredient.Id}");
            }

            existing!.Name = updateIngredient.Name;
            existing.Description = updateIngredient.Description;
            existing.Count = updateIngredient.Count;
            existing.Price = updateIngredient.Price;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            if (updateIngredient.Photo != null)
            {
                if (existing.PathPhoto != null) fileService.DeleteFile(existing.PathPhoto);
                existing.PathPhoto = await fileService.CreateFile(updateIngredient.Photo);
            }
            if (existing.Count > 2) existing.IsInReserve = true;
            else existing.IsInReserve = false;

            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated Ingredient by id:{updateIngredient.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteIngredientAsync

    public async Task<Response<bool>> DeleteIngredientAsync(Guid ingredientId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var Ingredient = await context.Ingredients.Where(x => x.Id == ingredientId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteIngredientAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return Ingredient == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"Ingredient not found by id:{ingredientId}")
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

