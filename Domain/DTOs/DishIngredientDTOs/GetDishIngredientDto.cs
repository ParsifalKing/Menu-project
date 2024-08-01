using Domain.DTOs.DishDTOs;
using Domain.DTOs.IngredientDTOs;
using Domain.Entities;

namespace Domain.DTOs.DishIngredientDTOs;

public class GetDishIngredientDto
{
    public Guid Id { get; set; }
    public Guid DishId { get; set; }
    public Guid IngredientId { get; set; }
    public double Quantity { get; set; }
    public required string Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GetDishDto? Dish { get; set; }
    public GetIngredientDto? Ingredient { get; set; }
}
