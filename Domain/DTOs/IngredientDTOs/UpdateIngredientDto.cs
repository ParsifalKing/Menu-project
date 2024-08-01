using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.IngredientDTOs;

public class UpdateIngredientDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required double Count { get; set; }
    public required decimal Price { get; set; }
    public IFormFile? Photo { get; set; }
}
