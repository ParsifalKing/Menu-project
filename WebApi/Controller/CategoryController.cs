using Domain.Constants;
using Domain.DTOs.CategoryDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.CategoryService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService _categoryService) : ControllerBase
{

    [HttpGet("Get-Categories")]
    [PermissionAuthorize(Permissions.Category.View)]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryFilter filter)
    {
        var response = await _categoryService.GetCategoriesAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Category-By{categoryId}")]
    [PermissionAuthorize(Permissions.Category.View)]
    public async Task<IActionResult> GetCategoryById(Guid categoryId)
    {
        var response = await _categoryService.GetCategoryByIdAsync(categoryId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-Category")]
    [PermissionAuthorize(Permissions.Category.Create)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategory)
    {
        var result = await _categoryService.CreateCategoryAsync(createCategory);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-Category")]
    [PermissionAuthorize(Permissions.Category.Edit)]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto updateCategory)
    {
        var result = await _categoryService.UpdateCategoryAsync(updateCategory);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("Delete-Category-{categoryId:Guid}")]
    [PermissionAuthorize(Permissions.Category.Delete)]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        var result = await _categoryService.DeleteCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

}

