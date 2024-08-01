using Domain.Enums;

namespace Domain.Filters;

public class OrderFilter : PaginationFilter
{
    public decimal? TotalAmount { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? DateOfPreparingOrder { get; set; }

}
