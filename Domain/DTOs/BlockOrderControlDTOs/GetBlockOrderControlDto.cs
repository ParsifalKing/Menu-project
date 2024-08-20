namespace Domain.DTOs.BlockOrderControlDTOs;

public class GetBlockOrderControlDto
{
    public int Id { get; set; } = 1;
    public bool IsBlocked { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
