namespace Domain.Entities;

public class OrderDetail : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid? DishId { get; set; }
    public Guid? DrinkId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
    public Dish? Dish { get; set; }
    public Drink? Drink { get; set; }
}
