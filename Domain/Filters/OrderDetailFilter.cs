namespace Domain.Filters;

public class OrderDetailFilter : PaginationFilter
{
    public Guid? OrderId { get; set; }
    public Guid? DishId { get; set; }
    public Guid? DrinkId { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
