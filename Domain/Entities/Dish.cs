namespace Domain.Entities;

public class Dish : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public float Calorie { get; set; }
    public int CookingTimeInMinutes { get; set; }
    public bool AreAllIngredients { get; set; }
    public string? PathPhoto { get; set; }

    public List<DishIngredient>? Ingredients { get; set; }
    public List<DishCategory>? Categories { get; set; }
    public List<OrderDetail>? OrderDetails { get; set; }

}
