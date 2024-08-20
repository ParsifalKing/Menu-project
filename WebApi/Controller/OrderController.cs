using Domain.Constants;
using Domain.Filters;
using Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services.OrderService;
using Domain.DTOs.OrderDTOs;
using Domain.DTOs.OrderDetailDTOs;
using Domain.DTOs.BlockOrderControlDTOs;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService _orderService) : ControllerBase
{

    [HttpGet("GetOrders")]
    [PermissionAuthorize(Permissions.Order.View)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderFilter filter)
    {
        var response = await _orderService.GetOrdersAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Order-By{orderId}")]
    [PermissionAuthorize(Permissions.Order.View)]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var response = await _orderService.GetOrderByIdAsync(orderId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("Get-Block-Order-Control")]
    [PermissionAuthorize(Permissions.Order.View)]
    public async Task<IActionResult> GetBlockOrderControl()
    {
        var response = await _orderService.GetBlockOrderControl();
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create-Order")]
    [PermissionAuthorize(Permissions.Order.Create)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderForControllerDto createOrderDto)
    {
        var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "sid")!.Value);

        var orderDetails = new List<CreateOrderDetailDto>();
        foreach (var orderDetail in createOrderDto.OrderDetails)
        {
            var mappedOrderDto = new CreateOrderDetailDto()
            {
                OrderId = new Guid(),
                Quantity = orderDetail.Quantity,
                DishId = orderDetail.DishId,
                DrinkId = orderDetail.DrinkId,
            };
            orderDetails.Add(mappedOrderDto);
        }

        var createOrder = new CreateOrderDto()
        {
            UserId = userId,
            OrderInfo = createOrderDto.OrderInfo,
            DateOfPreparingOrder = createOrderDto.DateOfPreparingOrder,
            OrderDetails = orderDetails,
        };
        var result = await _orderService.CreateOrderAsync(createOrder);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Block-Order")]
    [PermissionAuthorize(Permissions.Order.Edit)]
    public async Task<IActionResult> BlockOrder([FromQuery] UpdateBlockOrderControlDto updateBlockOrder)
    {
        var result = await _orderService.BlockOrderControl(updateBlockOrder);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPut("Update-Order")]
    [PermissionAuthorize(Permissions.Order.Edit)]
    public async Task<IActionResult> UpdateOrder([FromForm] UpdateOrderDto updateOrder)
    {
        var result = await _orderService.UpdateOrderAsync(updateOrder);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{orderId:Guid}")]
    [PermissionAuthorize(Permissions.Order.Delete)]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        var result = await _orderService.DeleteOrderAsync(orderId);
        return StatusCode(result.StatusCode, result);
    }

}
