namespace Domain.DTOs.OrderDetailDTOs;

public class CreateOrderDetailForControllerDto
{
    public Guid? DishId { get; set; }
    public Guid? DrinkId { get; set; }
    public required int Quantity { get; set; }
}
