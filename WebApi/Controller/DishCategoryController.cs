using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.DishCategoryService;
using Domain.DTOs.DishCategoryDTOs;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class DishCategoryController(IDishCategoryService _dishCategoryService) : ControllerBase
{

    [HttpGet("Get-DishCategory")]
    [PermissionAuthorize(Permissions.DishCategory.View)]
    public async Task<IActionResult> GetDishCategory([FromQuery] PaginationFilter filter)
    {
        var response = await _dishCategoryService.GetDishCategoryAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-DishCategory-By-Id")]
    [PermissionAuthorize(Permissions.DishCategory.View)]
    public async Task<IActionResult> GetDishCategoryById([FromQuery] DishCategoryDto dishCategoryDto)
    {
        var response = await _dishCategoryService.GetDishCategoryByIdAsync(dishCategoryDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-DishCategory")]
    [PermissionAuthorize(Permissions.DishCategory.Create)]
    public async Task<IActionResult> CreateDishCategory([FromBody] DishCategoryDto createDishCategory)
    {
        var result = await _dishCategoryService.CreateDishCategoryAsync(createDishCategory);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-DishCategory")]
    [PermissionAuthorize(Permissions.DishCategory.Edit)]
    public async Task<IActionResult> UpdateDishCategory([FromBody] UpdateDishCategoryDto updateDishCategory)
    {
        var result = await _dishCategoryService.UpdateDishCategoryAsync(updateDishCategory);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("Delete-DishCategory")]
    [PermissionAuthorize(Permissions.DishCategory.Delete)]
    public async Task<IActionResult> DeleteDishCategory(DishCategoryDto dishCategoryDto)
    {
        var result = await _dishCategoryService.DeleteDishCategoryAsync(dishCategoryDto);
        return StatusCode(result.StatusCode, result);
    }

}
