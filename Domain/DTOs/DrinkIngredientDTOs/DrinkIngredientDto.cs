namespace Domain.DTOs.DrinkIngredientDTOs;

public class DrinkIngredientDto
{
    public required Guid DrinkId { get; set; }
    public required Guid IngredientId { get; set; }
    public required float Quantity { get; set; }
    public string? Description { get; set; }
}
