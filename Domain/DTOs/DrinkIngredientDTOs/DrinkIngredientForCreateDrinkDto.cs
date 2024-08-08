namespace Domain.DTOs.DrinkIngredientDTOs;

public class DrinkIngredientForCreateDrinkDto
{
    public required Guid IngredientId { get; set; }
    public required float Quantity { get; set; }
    public string? Description { get; set; }
}
