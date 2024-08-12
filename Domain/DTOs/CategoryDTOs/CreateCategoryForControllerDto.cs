using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.CategoryDTOs;

public class CreateCategoryForControllerDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public IFormFile? Photo { get; set; }

    public string? CategoryDishesJson { get; set; }
}
