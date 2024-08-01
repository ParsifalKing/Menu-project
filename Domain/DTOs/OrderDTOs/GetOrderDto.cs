using Domain.Enums;

namespace Domain.DTOs.OrderDTOs;

public class GetOrderDto
{
    public Guid Id { get; set; }
    public required string OrderInfo { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public int OrderTimeInMinutes { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateOfPreparingOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
