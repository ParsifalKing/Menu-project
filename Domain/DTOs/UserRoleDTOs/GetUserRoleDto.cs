using Domain.DTOs.RoleDTOs;
using Domain.DTOs.UserDTOs;

namespace Domain.DTOs.UserRoleDTOs;

public class GetUserRoleDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GetRoleDto? Role { get; set; }
    public GetUserDto? User { get; set; }
}