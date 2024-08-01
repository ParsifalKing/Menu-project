namespace Domain.Entities;

public class DishIngredient : BaseEntity
{
    public Guid DishId { get; set; }
    public Guid IngredientId { get; set; }
    public double Quantity { get; set; }
    public string Description { get; set; } = null!;

    public Dish? Dish { get; set; }
    public Ingredient? Ingredient { get; set; }

}
