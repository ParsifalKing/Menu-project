using Domain.DTOs.DishCategoryDTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.CategoryDTOs;

public class CreateCategoryDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public IFormFile? Photo { get; set; }

    public List<DishCategoryForCreateCategoryDto>? CategoryDishes { get; set; }
}
