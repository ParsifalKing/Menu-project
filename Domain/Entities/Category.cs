namespace Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public List<DishCategory>? Dishes { get; set; }
}
