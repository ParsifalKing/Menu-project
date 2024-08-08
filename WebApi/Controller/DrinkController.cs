using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.DrinkService;
using Domain.DTOs.DrinkDTOs;
using Domain.DTOs.DrinkIngredientDTOs;
using Newtonsoft.Json;
using Domain.Entities;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class DrinkController(IDrinkService _drinkService) : ControllerBase
{

    [HttpGet("GetDrinks")]
    [PermissionAuthorize(Permissions.Drink.View)]
    public async Task<IActionResult> GetDrinks([FromQuery] DrinkFilter filter)
    {
        var response = await _drinkService.GetDrinksAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Drink-By{drinkId}")]
    [PermissionAuthorize(Permissions.Drink.View)]
    public async Task<IActionResult> GetDrinkById(Guid drinkId)
    {
        var response = await _drinkService.GetDrinkByIdAsync(drinkId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-Drink")]
    [PermissionAuthorize(Permissions.Drink.Create)]
    public async Task<IActionResult> CreateDrink([FromForm] CreateDrinkForControllerDto createDrinkDto)
    {

        List<DrinkIngredientForCreateDrinkDto>? drinkIngredient = null;
        if (!string.IsNullOrEmpty(createDrinkDto.DrinkIngredientsJson))
        {
            drinkIngredient = JsonConvert.DeserializeObject<List<DrinkIngredientForCreateDrinkDto>>(createDrinkDto.DrinkIngredientsJson);
        }

        var createDrink = new CreateDrinkDto()
        {
            CookingTimeInMinutes = createDrinkDto.CookingTimeInMinutes,
            Description = createDrinkDto.Description,
            Name = createDrinkDto.Name,
            Price = createDrinkDto.Price,
            Photo = createDrinkDto.Photo,
            DrinkIngredients = drinkIngredient,
        };

        var result = await _drinkService.CreateDrinkAsync(createDrink);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-Drink")]
    [PermissionAuthorize(Permissions.Drink.Edit)]
    public async Task<IActionResult> UpdateDrink([FromForm] UpdateDrinkForControllerDto updateDrinkDto)
    {

        List<DrinkIngredientDto>? drinkIngredients = null;
        if (!string.IsNullOrEmpty(updateDrinkDto.DrinkIngredientsJson))
        {
            drinkIngredients = JsonConvert.DeserializeObject<List<DrinkIngredientDto>>(updateDrinkDto.DrinkIngredientsJson);
        }

        var updateDrink = new UpdateDrinkDto()
        {
            CookingTimeInMinutes = updateDrinkDto.CookingTimeInMinutes,
            Description = updateDrinkDto.Description,
            Id = updateDrinkDto.Id,
            Name = updateDrinkDto.Name,
            Price = updateDrinkDto.Price,
            Photo = updateDrinkDto.Photo,
            DrinkIngredients = drinkIngredients,
        };

        var result = await _drinkService.UpdateDrinkAsync(updateDrink);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{drinkId:Guid}")]
    [PermissionAuthorize(Permissions.Drink.Delete)]
    public async Task<IActionResult> DeleteDrink(Guid drinkId)
    {
        var result = await _drinkService.DeleteDrinkAsync(drinkId);
        return StatusCode(result.StatusCode, result);
    }

}
