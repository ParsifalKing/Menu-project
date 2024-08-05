namespace Domain.Entities;

public class Drink : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int CookingTimeInMinutes { get; set; }
    public bool AreAllIngredients { get; set; }
    public string? PathPhoto { get; set; }

    public List<OrderDetail>? OrderDetails { get; set; }
    public List<DrinkIngredient>? Ingredients { get; set; }
}


