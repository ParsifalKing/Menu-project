using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.DrinkDTOs;

public class UpdateDrinkDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int Count { get; set; }
    public IFormFile? Photo { get; set; }
}
