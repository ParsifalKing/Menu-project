using System.Net;
using Domain.DTOs.BlockOrderControlDTOs;
using Domain.DTOs.NotificationDTOs;
using Domain.DTOs.OrderDetailDTOs;
using Domain.DTOs.OrderDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.CheckIngredientsService;
using Infrastructure.Services.NotificationService;
using Infrastructure.Services.OrderDetailService;
using Infrastructure.Services.TelegramService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services.OrderService;

public class OrderService(ILogger<OrderService> logger, DataContext context, IOrderDetailService orderDetailService,
ICheckIngredientsService checkIngredientsService, INotificationService notificationService, ITelegramService telegramService) : IOrderService
{


    #region GetOrdersAsync

    public async Task<PagedResponse<List<GetOrderDto>>> GetOrdersAsync(OrderFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetOrdersAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var orders = context.Orders.AsQueryable();

            if (filter.TotalAmount != null)
                orders = orders.Where(x => x.TotalAmount >= filter.TotalAmount);
            if (filter.OrderStatus != null)
                orders = orders.Where(x => x.OrderStatus == filter.OrderStatus);
            if (filter.UserId != null)
                orders = orders.Where(x => x.UserId == filter.UserId);
            if (filter.DateOfPreparingOrder != null)
                orders = orders.Where(x => x.DateOfPreparingOrder < filter.DateOfPreparingOrder).OrderBy(x => x.DateOfPreparingOrder);

            var response = await orders.Select(x => new GetOrderDto()
            {
                OrderInfo = x.OrderInfo,
                OrderStatus = x.OrderStatus,
                UserId = x.UserId,
                TotalAmount = x.TotalAmount,
                OrderTimeInMinutes = x.OrderTimeInMinutes,
                DateOfPreparingOrder = x.DateOfPreparingOrder,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalRecord = await orders.CountAsync();

            logger.LogInformation("Finished method GetOrdersAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetOrderDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetOrderDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region GetOrderByIdAsync

    public async Task<Response<GetOrderWithOrderDetail>> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            logger.LogInformation("Starting method GetOrderByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);

            var orderAndOrderDetails = await (from o in context.Orders
                                          where o.Id == orderId
                                          join od in context.OrderDetails on o.Id equals od.OrderId into OrderIngGroup
                                          from od in OrderIngGroup.DefaultIfEmpty()
                                          select new
                                          {
                                              Order = new GetOrderDto()
                                              { 
                                                  OrderInfo = o.OrderInfo,
                                                  TotalAmount = o.TotalAmount,
                                                  OrderTimeInMinutes = o.OrderTimeInMinutes,
                                                  UserId = o.UserId,
                                                  DateOfPreparingOrder = o.DateOfPreparingOrder,
                                                  OrderStatus = o.OrderStatus,
                                                  CreatedAt = o.CreatedAt,
                                                  UpdatedAt = o.UpdatedAt,
                                                  Id = o.Id,
                                              },
                                              OrderDetail = od != null ? new GetOrderDetailDto()
                                              {
                                                  
                                                  OrderId = od.OrderId,
                                                  DrinkId = od.DrinkId,
                                                  DishId = od.DishId,
                                                  Quantity = od.Quantity,
                                                  UnitPrice = od.UnitPrice,
                                                  CreatedAt = od.CreatedAt,
                                                  UpdatedAt = od.UpdatedAt,
                                                  Id = od.Id,
                                              } : null,
                                          }).ToListAsync();

            if (!orderAndOrderDetails.Any())
            {
                logger.LogWarning("Could not find Order with Id:{Id},time:{DateTimeNow}", orderId, DateTimeOffset.UtcNow);
                return new Response<GetOrderWithOrderDetail>(HttpStatusCode.BadRequest, $"Not found Drink by id:{orderId}");
            }

            var orderWithOrderDetail = new GetOrderWithOrderDetail()
            {
                Order = orderAndOrderDetails.First().Order,
                OrderDetails = orderAndOrderDetails.Where(x => x.OrderDetail != null)
                                                   .Select(x => x.OrderDetail).ToList(),
            };
            
            logger.LogInformation("Finished method GetDrinkByIdAsync at time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetOrderWithOrderDetail>(orderWithOrderDetail);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetOrderWithOrderDetail>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region GetBlockOrderControl

    public async Task<Response<GetBlockOrderControlDto>> GetBlockOrderControl()
    {
        try
        {
            logger.LogInformation("Starting method GetBlockOrderControl in time:{DateTime} ", DateTimeOffset.UtcNow);

            var blockOrderControl = await context.BlockOrderControl.FirstAsync(x => x.Id == 1);

            var blockOrderControlDto = new GetBlockOrderControlDto()
            {
                Id = blockOrderControl.Id,
                IsBlocked = blockOrderControl.IsBlocked,
                CreatedAt = blockOrderControl.CreatedAt,
                UpdatedAt = blockOrderControl.UpdatedAt,
            };

            logger.LogInformation("Finished method GetBlockOrderControl in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<GetBlockOrderControlDto>(blockOrderControlDto);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetBlockOrderControlDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region BlockOrdering


    public async Task<Response<string>> BlockOrderControl(UpdateBlockOrderControlDto blockOrderControlDto)
    {
        try
        {
            logger.LogInformation("Starting method BlockOrderControl in time:{DateTime} ", DateTimeOffset.UtcNow);

            var blockOrderControl = await context.BlockOrderControl.FirstAsync(x => x.Id == 1);
            blockOrderControl.IsBlocked = blockOrderControlDto.IsBlocked;
            blockOrderControl.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method BlockOrderControl in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully update BlockOrderControl");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region CreateOrderAsync

    public async Task<Response<string>> CreateOrderAsync(CreateOrderDto createOrder)
    {
        try
        {
            logger.LogInformation("Starting method CreateOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            var checkedBlockOrdering = await context.BlockOrderControl.FirstAsync(x => x.Id == 1);
            if (checkedBlockOrdering.IsBlocked)
            {
                logger.LogWarning("ordering is blocked now, in time:{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Sorry, but ordering blocked now");
            }
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == createOrder.UserId);
            foreach (var orderDetail in createOrder.OrderDetails)
            {
                if (orderDetail.Quantity <= 0)
                {
                    logger.LogWarning("Error 400, quantity of OrderDetail smaller than 0");
                    return new Response<string>(HttpStatusCode.BadRequest, "Detail of order have quantity smaller than 0");
                }
                if (orderDetail.DishId != null && orderDetail.DrinkId != null)
                {
                    logger.LogWarning("Error, one OrderDetail cannot have a dish and drink");
                    return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail cannot have a dish and drink");
                }
                if (orderDetail.DishId == null && orderDetail.DrinkId == null)
                {
                    logger.LogWarning("Error, one OrderDetail must at least have one detail ( dish or drink). Time:{DateTime}", DateTimeOffset.UtcNow);
                    return new Response<string>(HttpStatusCode.BadRequest, "Error, one OrderDetail must at least have one detail ( dish or drink).");
                }
                if (user == null)
                {
                    logger.LogWarning("Error 400, not found user with id:{userId}. In time:{DateTime}", createOrder.UserId, DateTime.UtcNow);
                    return new Response<string>(HttpStatusCode.BadRequest, $"Not found user with id:{createOrder.UserId}");
                }
            }
            if (createOrder.DateOfPreparingOrder < DateTime.UtcNow || createOrder.DateOfPreparingOrder > DateTime.UtcNow.AddDays(15))
            {
                logger.LogWarning("Error 400, incorrect input of property DateOfPreparingOrder : {Date}, in time:{DateTime}", createOrder.DateOfPreparingOrder, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Please, enter Date of preparing order correct. You can't order early that now and later that 15 days");
            }


            var newOrder = new Order()
            {
                OrderInfo = createOrder.OrderInfo,
                OrderStatus = Domain.Enums.OrderStatus.NotConfirmed,
                UserId = createOrder.UserId,
                OrderTimeInMinutes = 0,
                TotalAmount = 0,
                DateOfPreparingOrder = createOrder.DateOfPreparingOrder,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            var orderResult = await context.Orders.AddAsync(newOrder);
            await context.SaveChangesAsync();

            // Find order and Creating OrderDetails for order
            var order = await context.Orders.FirstAsync(x => x.Id == orderResult.Entity.Id && x.CreatedAt == newOrder.CreatedAt);
            foreach (var orderDetail in createOrder.OrderDetails)
            {
                orderDetail.OrderId = order.Id;
                var result = await orderDetailService.CreateOrderDetailAsync(orderDetail);
                if (result.StatusCode >= 400 && result.StatusCode <= 499) return new Response<string>(HttpStatusCode.BadRequest, "Error 400 during creating OrderDetail");
                if (result.StatusCode >= 500 && result.StatusCode <= 599) return new Response<string>(HttpStatusCode.InternalServerError, "Error 500 during creating OrderDetail");
            }

            // Updating (correcting) order properties
            var totalDishCookingTime = await (from od in context.OrderDetails
                                              join d in context.Dishes on od.DishId equals d.Id
                                              where od.OrderId == order.Id
                                              select d.CookingTimeInMinutes * od.Quantity)
                                          .SumAsync(x => x);
            var totalDrinkCookingTime = await (from od in context.OrderDetails
                                               join dr in context.Drinks on od.DrinkId equals dr.Id
                                               where od.OrderId == order.Id
                                               select dr.CookingTimeInMinutes * od.Quantity)
                                          .SumAsync(x => x);

            order.OrderTimeInMinutes = totalDishCookingTime + totalDrinkCookingTime + 5;

            order.TotalAmount = context.OrderDetails.Where(x => x.OrderId == order.Id).Sum(x => x.UnitPrice * x.Quantity);

            // Decrement count of Ingredients from stock 
            var orderDetails = await context.OrderDetails.Where(x => x.OrderId == order.Id).ToListAsync();
            foreach (var orderDetail in orderDetails)
            {
                if (orderDetail.DishId != null)
                {
                    var dishIngredient = (from i in context.Ingredients
                                          join di in context.DishesIngredients on i.Id equals di.IngredientId
                                          where di.DishId == orderDetail.DishId
                                          select new
                                          {
                                              Ingredients = i,
                                              DishIngredient = di,
                                          }).AsQueryable();

                    if (!dishIngredient.Any())
                    {
                        logger.LogWarning("Error 400, Dish has not any ingredients");
                        return new Response<string>(HttpStatusCode.BadRequest, "Ingredients of dish not found");
                    }
                    
                    Guid dishId = Guid.Empty;
                        foreach (var item in dishIngredient)
                        {
                            if (item.Ingredients.Count < item.DishIngredient.Quantity)
                            {
                                logger.LogWarning("Ups, count of ingredient with id:{IngredientId} not enough", item.Ingredients.Id);
                                return new Response<string>(HttpStatusCode.BadRequest, $"Ups, count of ingredient with id:{item.Ingredients.Id} not enough");
                            }
                            item.Ingredients.Count -= item.DishIngredient.Quantity * orderDetail.Quantity;
                            if (item.Ingredients.Count > 2) item.Ingredients.IsInReserve = true;
                            else item.Ingredients.IsInReserve = false;
                            dishId = item.DishIngredient.DishId;
                        }
                        await checkIngredientsService.CheckDishIngredients(dishId);
                    
                }
                else if (orderDetail.DrinkId != null)
                {
                    var drinkIngredient = (from i in context.Ingredients
                                           join di in context.DrinksIngredients on i.Id equals di.IngredientId
                                           where di.DrinkId == orderDetail.DrinkId
                                           select new
                                           {
                                               Ingredients = i,
                                               DrinkIngredient = di,
                                           }).AsQueryable();


                    if (drinkIngredient.Any())
                    {
                        Guid drinkId = Guid.Empty;
                        foreach (var item in drinkIngredient)
                        {
                            if (item.Ingredients.Count < item.DrinkIngredient.Quantity)
                            {
                                logger.LogWarning("Ups, count of ingredient with id:{IngredientId} not enough", item.Ingredients.Id);
                                return new Response<string>(HttpStatusCode.BadRequest, $"Ups, count of ingredient with id:{item.Ingredients.Id} not enough");
                            }
                            item.Ingredients.Count -= item.DrinkIngredient.Quantity * orderDetail.Quantity;
                            if (item.Ingredients.Count > 2) item.Ingredients.IsInReserve = true;
                            else item.Ingredients.IsInReserve = false;
                            drinkId = item.DrinkIngredient.DrinkId;
                        }
                        await checkIngredientsService.CheckDrinkIngredients(drinkId);
                    }
                }
            }
            await context.SaveChangesAsync();

            var notification = new CreateNotificationDto()
            {
                OrderId = order.Id,
                UserId = order.UserId,
            };
            await notificationService.CreateNotificationAsync(notification);
            await telegramService.SendMessageToAdmin($"Order id : {order.Id}  \nUsername of user who ordered : {user!.Username} \nThe phonenumber of user : {user.Phone}  \nOrder info : {order.OrderInfo} \nTotal amount of order : {order.TotalAmount}  \nStatus of order : {order.OrderStatus}  \nOrder completion time in minutes : {order.OrderTimeInMinutes}");

            logger.LogInformation("Finished method CreateOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Order by Id:{newOrder.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region UpdateOrderAsync

    public async Task<Response<string>> UpdateOrderAsync(UpdateOrderDto updateOrder)
    {
        try
        {
            logger.LogInformation("Starting method UpdateOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var existing = await context.Orders.FirstOrDefaultAsync(x => x.Id == updateOrder.Id);
            if (existing == null)
            {
                logger.LogWarning("Order not found by id:{Id},time:{Time}", updateOrder.Id, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not found Order by id:{updateOrder.Id}");
            }

            var totalCookingTime = await (from od in context.OrderDetails
                                          join d in context.Dishes on od.DishId equals d.Id
                                          where od.OrderId == updateOrder.Id
                                          select d.CookingTimeInMinutes * od.Quantity)
                                          .SumAsync(x => x);

            existing.OrderTimeInMinutes = totalCookingTime + 5;
            existing.TotalAmount = context.OrderDetails.Where(x => x.OrderId == updateOrder.Id).Sum(x => x.UnitPrice * x.Quantity);
            existing.OrderStatus = updateOrder.OrderStatus;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            var notification = new CreateNotificationDto()
            {
                OrderId = existing.Id,
                UserId = existing.UserId,
            };
            await notificationService.SendOrderNotificationAsync(notification);

            logger.LogInformation("Finished method UpdateOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully updated order by id:{updateOrder.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region DeleteOrderAsync

    public async Task<Response<bool>> DeleteOrderAsync(Guid orderId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var order = await context.Orders.Where(x => x.Id == orderId).ExecuteDeleteAsync();

            logger.LogInformation("Finished method DeleteOrderAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return order == 0
                ? new Response<bool>(HttpStatusCode.BadRequest, $"Order not found by id:{orderId}")
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
