using Domain.DTOs.NotificationDTOs;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Services.NotificationService;

public interface INotificationService
{
    Task<PagedResponse<List<GetNotificationDto>>> GetNotificationsAsync(NotificationFilter filter);
    Task<Response<GetNotificationDto>> GetNotificationByIdAsync(Guid notificationId);
    Task<Response<string>> CreateNotificationAsync(CreateNotificationDto createNotification);
    Task<Response<string>> SendOrderNotificationAsync(CreateNotificationDto createNotification);
}
