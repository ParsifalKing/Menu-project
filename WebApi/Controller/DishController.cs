using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.DishService;
using Domain.DTOs.DishDTOs;
using Domain.DTOs.DishIngredientDTOs;
using Newtonsoft.Json;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class DishController(IDishService _dishService) : ControllerBase
{

    [HttpGet("GetDishes")]
    [PermissionAuthorize(Permissions.Dish.View)]
    public async Task<IActionResult> GetDishes([FromQuery] DishFilter filter)
    {
        var response = await _dishService.GetDishesAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Dish-By{dishId}")]
    [PermissionAuthorize(Permissions.Dish.View)]
    public async Task<IActionResult> GetDishById(Guid dishId)
    {
        var response = await _dishService.GetDishByIdAsync(dishId);
        return StatusCode(response.StatusCode, response);
    }


    [HttpGet("GetDishesWithDrinks")]
    [PermissionAuthorize(Permissions.Dish.View)]
    public async Task<IActionResult> GetDishesWithDrinks()
    {
        var response = await _dishService.GetDishesWithDrinks();
        return StatusCode(response.StatusCode, response);
    }


    [HttpPost("Create-Dish")]
    [PermissionAuthorize(Permissions.Dish.Create)]
    public async Task<IActionResult> CreateDish([FromForm] CreateDishForControllerDto createDishDto)
    {
        List<DishIngredientForCreateDishDto>? dishIngredients = null;
        if (!string.IsNullOrEmpty(createDishDto.DishIngredientsJson))
        {
            dishIngredients = JsonConvert.DeserializeObject<List<DishIngredientForCreateDishDto>>(createDishDto.DishIngredientsJson);
        }

        var createDish = new CreateDishDto()
        {
            Name = createDishDto.Name,
            Calorie = createDishDto.Calorie,
            CookingTimeInMinutes = createDishDto.CookingTimeInMinutes,
            Description = createDishDto.Description,
            Price = createDishDto.Price,
            Photo = createDishDto.Photo,
            DishIngredients = dishIngredients,
        };

        var result = await _dishService.CreateDishAsync(createDish);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPut("Update-Dish")]
    [PermissionAuthorize(Permissions.Dish.Edit)]
    public async Task<IActionResult> UpdateDish([FromForm] UpdateDishForControllerDto updateDishDto)
    {

        List<DishIngredientDto>? dishIngredients = null;
        if (!string.IsNullOrEmpty(updateDishDto.DishIngredientsJson))
        {
            dishIngredients = JsonConvert.DeserializeObject<List<DishIngredientDto>>(updateDishDto.DishIngredientsJson);
        }

        var updateDish = new UpdateDishDto()
        {
            Id = updateDishDto.Id,
            Name = updateDishDto.Name,
            Calorie = updateDishDto.Calorie,
            CookingTimeInMinutes = updateDishDto.CookingTimeInMinutes,
            Description = updateDishDto.Description,
            Price = updateDishDto.Price,
            Photo = updateDishDto.Photo,
            DishIngredients = dishIngredients,
        };

        var result = await _dishService.UpdateDishAsync(updateDish);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{dishId:Guid}")]
    [PermissionAuthorize(Permissions.Dish.Delete)]
    public async Task<IActionResult> DeleteDish(Guid dishId)
    {
        var result = await _dishService.DeleteDishAsync(dishId);
        return StatusCode(result.StatusCode, result);
    }

}
