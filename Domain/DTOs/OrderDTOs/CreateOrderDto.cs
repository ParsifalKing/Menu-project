using Domain.DTOs.OrderDetailDTOs;
using Domain.Enums;

namespace Domain.DTOs.OrderDTOs;

public class CreateOrderDto
{
    public required string OrderInfo { get; set; }
    public required DateTime DateOfPreparingOrder { get; set; }
    public required Guid UserId { get; set; }

    public required List<CreateOrderDetailDto> OrderDetails { get; set; }
}
