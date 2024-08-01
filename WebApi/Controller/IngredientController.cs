using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.IngredientService;
using Domain.DTOs.IngredientDTOs;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class IngredientController(IIngredientService _IngredientService) : ControllerBase
{

    [HttpGet("GetIngredients")]
    [PermissionAuthorize(Permissions.Ingredient.View)]
    public async Task<IActionResult> GetIngredients([FromQuery] IngredientFIlter filter)
    {
        var response = await _IngredientService.GetIngredientsAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Ingredient-By{IngredientId}")]
    [PermissionAuthorize(Permissions.Ingredient.View)]
    public async Task<IActionResult> GetIngredientById(Guid IngredientId)
    {
        var response = await _IngredientService.GetIngredientByIdAsync(IngredientId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-Ingredient")]
    [PermissionAuthorize(Permissions.Ingredient.Create)]
    public async Task<IActionResult> CreateIngredient([FromForm] CreateIngredientDto createIngredient)
    {
        var result = await _IngredientService.CreateIngredientAsync(createIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-Ingredient")]
    [PermissionAuthorize(Permissions.Ingredient.Edit)]
    public async Task<IActionResult> UpdateIngredient([FromForm] UpdateIngredientDto updateIngredient)
    {
        var result = await _IngredientService.UpdateIngredientAsync(updateIngredient);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{IngredientId:Guid}")]
    [PermissionAuthorize(Permissions.Ingredient.Delete)]
    public async Task<IActionResult> DeleteIngredient(Guid IngredientId)
    {
        var result = await _IngredientService.DeleteIngredientAsync(IngredientId);
        return StatusCode(result.StatusCode, result);
    }

}
