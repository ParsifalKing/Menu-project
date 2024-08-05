using Domain.DTOs.DrinkDTOs;
using Domain.DTOs.IngredientDTOs;

namespace Domain.DTOs.DrinkIngredientDTOs;

public class GetDrinkIngredientDto
{
    public Guid Id { get; set; }
    public Guid DrinkId { get; set; }
    public Guid IngredientId { get; set; }
    public float Quantity { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GetDrinkDto? Drink { get; set; }
    public GetIngredientDto? Ingredient { get; set; }
}
