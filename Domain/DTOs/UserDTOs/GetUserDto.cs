namespace Domain.DTOs.UserDTOs;

public class GetUserDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Status { get; set; }
    public string? PathPhoto { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}