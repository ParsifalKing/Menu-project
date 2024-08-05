namespace Domain.Entities;

public class DrinkIngredient : BaseEntity
{
    public Guid DrinkId { get; set; }
    public Guid IngredientId { get; set; }
    public float Quantity { get; set; }
    public string? Description { get; set; }

    public Drink? Drink { get; set; }
    public Ingredient? Ingredient { get; set; }
}
