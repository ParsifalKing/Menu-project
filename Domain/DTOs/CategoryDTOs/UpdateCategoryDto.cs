using Domain.DTOs.DishCategoryDTOs;
using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.CategoryDTOs;

public class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public IFormFile? Photo { get; set; }

    public List<DishCategoryDto>? CategoryDishes { get; set; }

}
