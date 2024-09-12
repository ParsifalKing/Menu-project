using Domain.Constants;
using Domain.DTOs.UserRoleDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.UserRoleService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserRoleController(IUserRoleService userRoleService) : ControllerBase
{
    [HttpGet("User-Roles")]
    [PermissionAuthorize(Permissions.UserRole.View)]
    public async Task<IActionResult> GetUserUserRoles([FromQuery] PaginationFilter filter)
    {
        var res1 = await userRoleService.GetUserRolesAsync(filter);
        return StatusCode(res1.StatusCode, res1);
    }

    [HttpGet("Get-By-Id")]
    [PermissionAuthorize(Permissions.UserRole.View)]
    public async Task<IActionResult> GetUserRoleById([FromQuery] UserRoleDto userRole)
    {
        var res1 = await userRoleService.GetUserRoleByIdAsync(userRole);
        return StatusCode(res1.StatusCode, res1);
    }

    [HttpPost("Create")]
    [PermissionAuthorize(Permissions.UserRole.Create)]
    public async Task<IActionResult> CreateUserRole([FromBody] UserRoleDto createUserRole)
    {
        var result = await userRoleService.CreateUserRoleAsync(createUserRole);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("Delete")]
    [PermissionAuthorize(Permissions.UserRole.Delete)]
    public async Task<IActionResult> DeleteUserRole([FromBody] UserRoleDto userRole)
    {
        var result = await userRoleService.DeleteUserRoleAsync(userRole);
        return StatusCode(result.StatusCode, result);
    }
}