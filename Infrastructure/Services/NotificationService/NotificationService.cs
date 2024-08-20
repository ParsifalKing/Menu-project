using System.Net;
using Domain.DTOs.EmailDTOs;
using Domain.DTOs.NotificationDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.EmailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit.Text;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Infrastructure.Services.NotificationService;

public class NotificationService(ILogger<NotificationService> logger, DataContext context, IEmailService emailService) : INotificationService
{
    
    #region GetNotificationsAsync

    public async Task<PagedResponse<List<GetNotificationDto>>> GetNotificationsAsync(NotificationFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method {GetNotificationsAsync} in time:{DateTime} ", "GetNotificationsAsync",
                DateTimeOffset.UtcNow);
            var Notifications = context.Notifications.AsQueryable();

            if (filter.SendDate != null)
                Notifications = Notifications.Where(x => x.SendDate >= filter.SendDate);

            var response = await Notifications.Select(x => new GetNotificationDto()
            {
                SendDate = x.SendDate,
                Id = x.Id,
                UserId = x.UserId,
                OrderId = x.OrderId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            var totalRecord = await Notifications.CountAsync();

            logger.LogInformation("Finished method {GetNotificationsAsync} in time:{DateTime} ", "GetNotificationsAsync",
                DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetNotificationDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetNotificationDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetNotificationByIdAsync

    public async Task<Response<GetNotificationDto>> GetNotificationByIdAsync(Guid notificationId)
    {
        try
        {
            logger.LogInformation("Starting method {GetNotificationByIdAsync} in time:{DateTime} ", "GetNotificationByIdAsync",
                DateTimeOffset.UtcNow);

            var existing = await context.Notifications.Select(x => new GetNotificationDto()
            {
                SendDate = x.SendDate,
                Id = x.Id,
                UserId = x.UserId,
                OrderId = x.OrderId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).FirstOrDefaultAsync(x => x.Id == notificationId);

            if (existing is null)
            {
                logger.LogWarning("Could not find Notification with Id:{Id},time:{DateTimeNow}", notificationId, DateTimeOffset.UtcNow);
                return new Response<GetNotificationDto>(HttpStatusCode.BadRequest, $"Not found Notification by id:{notificationId}");
            }


            logger.LogInformation("Finished method {GetNotificationByIdAsync} in time:{DateTime} ", "GetNotificationByIdAsync",
                DateTimeOffset.UtcNow);
            return new Response<GetNotificationDto>(existing);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetNotificationDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region CreateNotificationAsync

    public async Task<Response<string>> CreateNotificationAsync(CreateNotificationDto createNotification)
    {
        try
        {
            logger.LogInformation("Starting method {CreateNotificationAsync} in time:{DateTime} ", "CreateNotificationAsync",
                DateTimeOffset.UtcNow);

            var newNotification = new Notification()
            {
                UserId = createNotification.UserId,
                OrderId = createNotification.OrderId,
                SendDate = DateTime.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await context.Notifications.AddAsync(newNotification);
            await context.SaveChangesAsync();

            await SendOrderNotificationAsync(createNotification);

            logger.LogInformation("Finished method {CreateNotificationAsync} in time:{DateTime} ", "CreateNotificationAsync",
                DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created Notification by Id:{newNotification.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region SendOrderNotificationAsync

    public async Task<Response<string>> SendOrderNotificationAsync(CreateNotificationDto createNotification)
    {
        try
        {
            logger.LogInformation("Starting method SendNotificationAsync in time {DateTime}", DateTime.UtcNow);
            var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == createNotification.OrderId);
            if (order == null)
            {
                logger.LogWarning("Order by id:{OrderId} not found at time : {DateTime}", createNotification.OrderId, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Order by id:{createNotification.OrderId} not found");
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == createNotification.UserId);
            if (user == null)
            {
                logger.LogWarning("User with id {UserId} not found , time={DateTimeNow}", createNotification.UserId, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"User with id:{createNotification.UserId} not found ");
            }

            var orderInfoForAdmin = $"Order Id : {order.Id} <br>  Order info : {order!.OrderInfo} <br> Total amount of order : {order.TotalAmount} <br> Status of order : {order.OrderStatus} <br> Order completion time in minutes : {order.OrderTimeInMinutes} <br> Username of user who ordered : {user.Username} <br> The phonenumber of user : {user.Phone}";

            var orderInfoForUser = $"Order info : {order!.OrderInfo} <br> Total amount of order : {order.TotalAmount} <br> Status of order : {order.OrderStatus} <br> Order completion time in minutes : {order.OrderTimeInMinutes} <br> Username of user who ordered : {user.Username} <br> The phonenumber of user : {user.Phone}";

            await emailService.SendEmail(new EmailMessageDto(new[] { "ymmumenu@gmail.com" }, "All information about order ",
                $"<h1>{orderInfoForAdmin}</h1>"), TextFormat.Html);
            await emailService.SendEmail(new EmailMessageDto(new[] { user.Email }, "All information about order ",
            $"<h1>{orderInfoForUser}</h1>"), TextFormat.Html);

            logger.LogInformation("Finished method SendNotificationAsync in time {DateTime}", DateTime.UtcNow);
            return new Response<string>("Notification successfully sent!!! ");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

}

