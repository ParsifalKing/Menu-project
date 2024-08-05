using Domain.Constants;
using Domain.DTOs.DrinkIngredientDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.DrinkIngredientService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class DrinkIngredientController(IDrinkIngredientService _drinkIngredientService) : ControllerBase
{

    [HttpGet("Get-DrinkIngredient")]
    [PermissionAuthorize(Permissions.DrinkIngredient.View)]
    public async Task<IActionResult> GetDrinkIngredient([FromQuery] PaginationFilter filter)
    {
        var response = await _drinkIngredientService.GetDrinkIngredientAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-DrinkIngredient-By-Id")]
    [PermissionAuthorize(Permissions.DrinkIngredient.View)]
    public async Task<IActionResult> GetDrinkIngredientById(Guid drinkId, Guid ingredientId)
    {
        var response = await _drinkIngredientService.GetDrinkIngredientByIdAsync(drinkId, ingredientId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-DrinkIngredient")]
    [PermissionAuthorize(Permissions.DrinkIngredient.Create)]
    public async Task<IActionResult> CreateDrinkIngredient([FromBody] DrinkIngredientDto createDrinkIngredient)
    {
        var result = await _drinkIngredientService.CreateDrinkIngredientAsync(createDrinkIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-DrinkIngredient")]
    [PermissionAuthorize(Permissions.DrinkIngredient.Edit)]
    public async Task<IActionResult> UpdateDrinkIngredient([FromBody] UpdateDrinkIngredientDto updateDrinkIngredient)
    {
        var result = await _drinkIngredientService.UpdateDrinkIngredientAsync(updateDrinkIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("Delete-DrinkIngredient")]
    [PermissionAuthorize(Permissions.DrinkIngredient.Delete)]
    public async Task<IActionResult> DeleteDrinkIngredient(Guid drinkId, Guid ingredientId)
    {
        var result = await _drinkIngredientService.DeleteDrinkIngredientAsync(drinkId, ingredientId);
        return StatusCode(result.StatusCode, result);
    }

}

