using System.Net;
using Domain.DTOs.OrderDetailDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services.OrderDetailService;

public class OrderDetailService(ILogger<OrderDetailService> logger, DataContext context) : IOrderDetailService
{

    #region GetOrderDetailsAsync

    public async Task<PagedResponse<List<GetOrderDetailDto>>> GetOrderDetailsAsync(OrderDetailFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetOrderDetailsAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var orderDetails = context.OrderDetails.AsQueryable();

            if (filter.OrderId != null)
                orderDetails = orderDetails.Where(x => x.OrderId == filter.OrderId);
            if (filter.DishId != null)
                orderDetails = orderDetails.Where(x => x.DishId == filter.DishId);
            if (filter.DrinkId != null)
                orderDetails = orderDetails.Where(x => x.DrinkId == filter.DrinkId);
            if (filter.Quantity != null)
                orderDetails = orderDetails.Where(x => x.Quantity >= filter.Quantity);
            if (filter.UnitPrice != null)
                orderDetails = orderDetails.Where(x => x.UnitPrice >= filter.UnitPrice);

            var response = await orderDetails.Select(x => new GetOrderDetailDto()
            {
                OrderId = x.OrderId,
                DishId = x.DishId,
                DrinkId = x.DrinkId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await orderDetails.CountAsync();

            logger.LogInformation("Finished method GetOrderDetailsAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetOrderDetailDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetOrderDetailDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    
    #region GetOrderDetailByIdAsync

    public async Task<Response<GetOrderDetailDto>> GetOrderDetailByIdAsync(Guid orderDetailId)
    {
        try
        {
            logger.LogInformation("Starting method GetOrderDetailByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.OrderDetails.Select(x => new GetOrderDetailDto()
            {
                OrderId = x.OrderId,
                DishId = x.DishId,
                DrinkId = x.DrinkId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).FirstOrDefaultAsync(x => x.Id == orderDetailId);

            if (existing is null)
            {
                logger.LogWarning("Could not find OrderDetail with Id:{Id},time:{DateTimeNow}", orderDetailId, DateTimeOffset.UtcNow);
                return new Response<GetOrderDetailDto>(HttpStatusCode.BadRequest, $"Not found OrderDetail by id:{orderDetailId}");
            }

            logger.LogInformation("Finished method GetOrderDetailByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetOrderDetailDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetOrderDetailDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    
    #region CreateOrderDetailAsync

    public async Task<Response<string>> CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetail)
    {
        try
        {
            logger.LogInformation("Starting method CreateOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            if (createOrderDetail.DishId != null && createOrderDetail.DrinkId != null)
            {
                logger.LogWarning("Error, one OrderDetail cannot have a dish and drink");
                return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail cannot have a dish and drink");
            }
            if (createOrderDetail.DishId == null && createOrderDetail.DrinkId == null)
            {
                logger.LogWarning("Error, one OrderDetail must at least have one detail ( dish or drink). Time:{DateTime}", DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail must at least have one detail ( dish or drink).");
            }
            decimal unitPrice = 0;
            if (createOrderDetail.DishId != null) unitPrice = context.Dishes.First(x => x.Id == createOrderDetail.DishId).Price;
            else if (createOrderDetail.DrinkId != null) unitPrice = context.Drinks.First(x => x.Id == createOrderDetail.DrinkId).Price;

            var newOrderDetail = new OrderDetail()
            {
                UnitPrice = unitPrice,
                OrderId = createOrderDetail.OrderId,
                DrinkId = createOrderDetail.DrinkId,
                DishId = createOrderDetail.DishId,
                Quantity = createOrderDetail.Quantity,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await context.OrderDetails.AddAsync(newOrderDetail);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method CreateOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created OrderDetail by Id:{newOrderDetail.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    
    #region UpdateOrderDetailAsync

    public async Task<Response<string>> UpdateOrderDetailAsync(UpdateOrderDetailDto updateOrderDetail)
    {
        try
        {
            logger.LogInformation("Starting method UpdateOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.OrderDetails.FirstOrDefaultAsync(x => x.Id == updateOrderDetail.Id);
            if (existing == null)
            {
                logger.LogWarning("OrderDetail not found by id:{Id},time:{Time}", updateOrderDetail.Id, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not found OrderDetail by id:{updateOrderDetail.Id}");
            }
            if (updateOrderDetail.DishId != null && updateOrderDetail.DrinkId != null)
            {
                logger.LogWarning("Error, one OrderDetail cannot have a dish and drink");
                return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail cannot have a dish and drink");
            }
            if (updateOrderDetail.DishId == null && updateOrderDetail.DrinkId == null)
            {
                logger.LogWarning("Error, one OrderDetail must at least have one detail ( dish or drink). Time:{DateTime}", DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail must at least have one detail ( dish or drink).");
            }
            decimal unitPrice = 0;
            if (updateOrderDetail.DishId != null) unitPrice = context.Dishes.First(x => x.Id == updateOrderDetail.DishId).Price;
            else if (updateOrderDetail.DrinkId != null) unitPrice = context.Drinks.First(x => x.Id == updateOrderDetail.DrinkId).Price;


            existing.UnitPrice = unitPrice;
            existing.OrderId = updateOrderDetail.OrderId;
            existing.DishId = updateOrderDetail.DishId;
            existing.DrinkId = updateOrderDetail.DrinkId;
            existing.Quantity = updateOrderDetail.Quantity;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method UpdateOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated OrderDetail by id:{updateOrderDetail.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    
    #region DeleteOrderDetailAsync

    public async Task<Response<bool>> DeleteOrderDetailAsync(Guid orderDetailId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var orderDetail = await context.OrderDetails.Where(x => x.Id == orderDetailId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteOrderDetailAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return orderDetail == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"OrderDetail not found by id:{orderDetailId}")
                : new Response<bool>(true);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<bool>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


}
