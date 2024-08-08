namespace Domain.DTOs.DishIngredientDTOs;

public class DishIngredientForCreateDishDto
{
    public required Guid IngredientId { get; set; }
    public required float Quantity { get; set; }
    public string? Description { get; set; }
}
