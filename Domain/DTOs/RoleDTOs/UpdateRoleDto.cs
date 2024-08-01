namespace Domain.DTOs.RoleDTOs;

public class UpdateRoleDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}