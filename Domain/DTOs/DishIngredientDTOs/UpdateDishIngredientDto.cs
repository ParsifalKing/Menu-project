namespace Domain.DTOs.DishIngredientDTOs;

public class UpdateDishIngredientDto
{
    public required Guid Id { get; set; }
    public required Guid DishId { get; set; }
    public required Guid IngredientId { get; set; }
    public required float Quantity { get; set; }
    public string? Description { get; set; }
}
