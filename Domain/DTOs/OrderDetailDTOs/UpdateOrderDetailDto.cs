namespace Domain.DTOs.OrderDetailDTOs;

public class UpdateOrderDetailDto
{
    public required Guid Id { get; set; }
    public required Guid OrderId { get; set; }
    public Guid? DishId { get; set; }
    public Guid? DrinkId { get; set; }
    public required int Quantity { get; set; }
}
