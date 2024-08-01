namespace Domain.Entities;

public class Notification : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SendDate { get; set; }

    public Order? Order { get; set; }
    public User? User { get; set; }
}
