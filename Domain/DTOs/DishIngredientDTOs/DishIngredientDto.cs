namespace Domain.DTOs.DishIngredientDTOs;

public class DishIngredientDto
{
    public required Guid DishId { get; set; }
    public required Guid IngredientId { get; set; }
    public required float Quantity { get; set; }
    public string? Description { get; set; }
}
