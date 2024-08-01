using Domain.Enums;

namespace Domain.DTOs.OrderDTOs;

public class UpdateOrderDto
{
    public required Guid Id { get; set; }
    public required OrderStatus OrderStatus { get; set; }

}
