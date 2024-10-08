using Domain.Constants;
using Domain.DTOs.RoleDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.RoleService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet("Roles")]
    [PermissionAuthorize(Permissions.Role.View)]
    public async Task<IActionResult> GetRoles([FromQuery] RoleFilter filter)
    {
        var response = await roleService.GetRolesAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{roleId:Guid}")]
    [PermissionAuthorize(Permissions.Role.View)]
    public async Task<IActionResult> GetRoleById(Guid roleId)
    {
        var response = await roleService.GetRoleByIdAsync(roleId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Create")]
    [PermissionAuthorize(Permissions.Role.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRole)
    {
        var result = await roleService.CreateRoleAsync(createRole);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPut("Update")]
    [PermissionAuthorize(Permissions.Role.Edit)]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto updateRole)
    {
        var result = await roleService.UpdateRoleAsync(updateRole);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{roleId:Guid}")]
    [PermissionAuthorize(Permissions.Role.Delete)]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var result = await roleService.DeleteRoleAsync(roleId);
        return StatusCode(result.StatusCode, result);
    }
}

