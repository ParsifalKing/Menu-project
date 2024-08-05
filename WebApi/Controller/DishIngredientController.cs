using Domain.Constants;
using Domain.DTOs.DishIngredientDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.DishIngredientService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class DishIngredientController(IDishIngredientService _dishIngredientService) : ControllerBase
{

    [HttpGet("Get-DishIngredient")]
    [PermissionAuthorize(Permissions.DishIngredient.View)]
    public async Task<IActionResult> GetDishIngredient([FromQuery] PaginationFilter filter)
    {
        var response = await _dishIngredientService.GetDishIngredientAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-DishIngredient-By-Id")]
    [PermissionAuthorize(Permissions.DishIngredient.View)]
    public async Task<IActionResult> GetDishIngredientById(Guid dishId, Guid ingredientId)
    {
        var response = await _dishIngredientService.GetDishIngredientByIdAsync(dishId, ingredientId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-DishIngredient")]
    [PermissionAuthorize(Permissions.DishIngredient.Create)]
    public async Task<IActionResult> CreateDishIngredient([FromBody] DishIngredientDto createDishIngredient)
    {
        var result = await _dishIngredientService.CreateDishIngredientAsync(createDishIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-DishIngredient")]
    [PermissionAuthorize(Permissions.DishIngredient.Edit)]
    public async Task<IActionResult> UpdateDishIngredient([FromBody] UpdateDishIngredientDto updateDishIngredient)
    {
        var result = await _dishIngredientService.UpdateDishIngredientAsync(updateDishIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("Delete-DishIngredient")]
    [PermissionAuthorize(Permissions.DishIngredient.Delete)]
    public async Task<IActionResult> DeleteDishIngredient(Guid dishId, Guid ingredientId)
    {
        var result = await _dishIngredientService.DeleteDishIngredientAsync(dishId, ingredientId);
        return StatusCode(result.StatusCode, result);
    }

}

