using Domain.DTOs.OrderDetailDTOs;

namespace Domain.DTOs.OrderDTOs;

public class CreateOrderForControllerDto
{
    public required string OrderInfo { get; set; }
    public required DateTime DateOfPreparingOrder { get; set; }

    public required List<CreateOrderDetailForControllerDto> OrderDetails { get; set; }
}
