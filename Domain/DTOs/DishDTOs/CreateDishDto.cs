using Domain.DTOs.DishCategoryDTOs;
using Domain.DTOs.DishIngredientDTOs;
using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.DishDTOs;

public class CreateDishDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int CookingTimeInMinutes { get; set; }
    public required float Calorie { get; set; }
    public IFormFile? Photo { get; set; }

    public List<DishIngredientForCreateDishDto>? DishIngredients { get; set; }
    public List<DishCategoryForCreateDishDto>? DishCategories { get; set; }
}
