using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.DishDTOs;

public class CreateDishForControllerDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int CookingTimeInMinutes { get; set; }
    public required float Calorie { get; set; }
    public IFormFile? Photo { get; set; }

    public string? DishIngredientsJson { get; set; }
    public string? DishCategoriesJson { get; set; }

}
