namespace Domain.DTOs.OrderDetailDTOs;

public class CreateOrderDetailDto
{
    public required Guid OrderId { get; set; }
    public Guid? DishId { get; set; } = null;
    public Guid? DrinkId { get; set; } = null;
    public required int Quantity { get; set; }
}
