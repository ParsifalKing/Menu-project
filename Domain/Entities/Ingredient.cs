namespace Domain.Entities;

public class Ingredient : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Count { get; set; }
    public decimal Price { get; set; }
    public bool IsInReserve { get; set; }
    public string? PathPhoto { get; set; }

    public List<DishIngredient>? Dishes { get; set; }
}
