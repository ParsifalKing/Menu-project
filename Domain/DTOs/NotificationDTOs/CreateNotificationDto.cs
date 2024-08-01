namespace Domain.DTOs.NotificationDTOs;

public class CreateNotificationDto
{
    public required Guid OrderId { get; set; }
    public required Guid UserId { get; set; }

}
