namespace Domain.DTOs.BlockOrderControlDTOs;

public class GetBlockOrderControlDto
{
    public Guid Id { get; set; }
    public bool IsBlocked { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
