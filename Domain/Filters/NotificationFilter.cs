namespace Domain.Filters;

public class NotificationFilter : PaginationFilter
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? SendDate { get; set; }
}
