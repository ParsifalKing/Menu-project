using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.DishDTOs;

public class CreateDishDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int CookingTimeInMinutes { get; set; }
    public IFormFile? Photo { get; set; }
}
