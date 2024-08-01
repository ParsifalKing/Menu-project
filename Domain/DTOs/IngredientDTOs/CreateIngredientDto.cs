using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.IngredientDTOs;

public class CreateIngredientDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required double Count { get; set; }
    public required decimal Price { get; set; }
    public IFormFile? Photo { get; set; }
}
