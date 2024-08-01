namespace Domain.DTOs.BlockOrderControlDTOs;

public class UpdateBlockOrderControlDto
{
    public required Guid Id { get; set; }
    public required bool IsBlocked { get; set; }
}
