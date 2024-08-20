using Domain.DTOs.OrderDetailDTOs;

namespace Domain.DTOs.OrderDTOs;

public class GetOrderWithOrderDetail
{
    public GetOrderDto Order { get; set; } = null!;
    public List<GetOrderDetailDto>? OrderDetails { get; set; }
}