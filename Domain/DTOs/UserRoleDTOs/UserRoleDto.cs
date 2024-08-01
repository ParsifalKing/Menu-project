namespace Domain.DTOs.UserRoleDTOs;

public class UserRoleDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}