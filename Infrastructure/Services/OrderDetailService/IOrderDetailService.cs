using Domain.DTOs.OrderDetailDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.OrderDetailService;

public interface IOrderDetailService
{
    Task<PagedResponse<List<GetOrderDetailDto>>> GetOrderDetailsAsync(OrderDetailFilter filter);
    Task<Response<GetOrderDetailDto>> GetOrderDetailByIdAsync(Guid orderDetailId);
    Task<Response<string>> CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetail);
    Task<Response<string>> UpdateOrderDetailAsync(UpdateOrderDetailDto updateOrderDetail);
    Task<Response<bool>> DeleteOrderDetailAsync(Guid orderDetailId);
}
