using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.DishDTOs;

public class UpdateDishForControllerDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required float Calorie { get; set; }
    public required int CookingTimeInMinutes { get; set; }
    public IFormFile? Photo { get; set; }

    public string? DishIngredientsJson { get; set; }
}
