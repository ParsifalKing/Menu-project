using Domain.DTOs.CategoryDTOs;
using Domain.DTOs.DishDTOs;
using Domain.Entities;

namespace Domain.DTOs.DishCategoryDTOs;

public class GetDishCategoryDto
{
    public Guid Id { get; set; }
    public Guid DishId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GetDishDto? Dish { get; set; }
    public GetCategoryDto? Category { get; set; }
}
