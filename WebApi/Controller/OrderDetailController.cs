using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.OrderDetailService;
using Domain.DTOs.OrderDetailDTOs;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrderDetailController(IOrderDetailService _orderDetailService) : ControllerBase
{

    [HttpGet("GetOrderDetails")]
    [PermissionAuthorize(Permissions.OrderDetail.View)]
    public async Task<IActionResult> GetOrderDetails([FromQuery] OrderDetailFilter filter)
    {
        var response = await _orderDetailService.GetOrderDetailsAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-OrderDetail-By{orderDetailId}")]
    [PermissionAuthorize(Permissions.OrderDetail.View)]
    public async Task<IActionResult> GetOrderDetailById(Guid orderDetailId)
    {
        var response = await _orderDetailService.GetOrderDetailByIdAsync(orderDetailId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-OrderDetail")]
    [PermissionAuthorize(Permissions.OrderDetail.Create)]
    public async Task<IActionResult> CreateOrderDetail([FromForm] CreateOrderDetailDto createOrderDetail)
    {
        var result = await _orderDetailService.CreateOrderDetailAsync(createOrderDetail);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Update-OrderDetail")]
    [PermissionAuthorize(Permissions.OrderDetail.Edit)]
    public async Task<IActionResult> UpdateOrderDetail([FromForm] UpdateOrderDetailDto updateOrderDetail)
    {
        var result = await _orderDetailService.UpdateOrderDetailAsync(updateOrderDetail);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{orderDetailId:Guid}")]
    [PermissionAuthorize(Permissions.OrderDetail.Delete)]
    public async Task<IActionResult> DeleteOrderDetail(Guid orderDetailId)
    {
        var result = await _orderDetailService.DeleteOrderDetailAsync(orderDetailId);
        return StatusCode(result.StatusCode, result);
    }

}
