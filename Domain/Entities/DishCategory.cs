namespace Domain.Entities;

public class DishCategory : BaseEntity
{
    public Guid DishId { get; set; }
    public Guid CategoryId { get; set; }

    public Dish? Dish { get; set; }
    public Category? Category { get; set; }
}
