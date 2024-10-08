namespace Domain.Entities;

public class DishIngredient : BaseEntity
{
    public Guid DishId { get; set; }
    public Guid IngredientId { get; set; }
    public float Quantity { get; set; }
    public string? Description { get; set; }

    public Dish? Dish { get; set; }
    public Ingredient? Ingredient { get; set; }

}
