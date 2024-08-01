using System.Reflection.Metadata;
using Domain.Enums;

namespace Domain.Entities;

public class Order : BaseEntity
{
    public string OrderInfo { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public int OrderTimeInMinutes { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateOfPreparingOrder { get; set; }

    public List<OrderDetail>? OrderDetails { get; set; }
    public User? User { get; set; }
    public Notification? Notification { get; set; }
}
