using Domain.Constants;
using Domain.DTOs.UserDTOs;
using Domain.Filters;
using Infrastructure.Permissions;
using Infrastructure.Services.UserService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase

{
    [HttpGet("Users")]
    [PermissionAuthorize(Permissions.User.View)]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilter filter)
    {
        var res1 = await userService.GetUsersAsync(filter);
        return StatusCode(res1.StatusCode, res1);
    }

    [HttpGet("{userId:Guid}")]
    [PermissionAuthorize(Permissions.User.View)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var res1 = await userService.GetUserByIdAsync(userId);
        return StatusCode(res1.StatusCode, res1);
    }


    [HttpPut("Update")]
    [PermissionAuthorize(Permissions.User.Edit)]
    public async Task<IActionResult> UpdateUser([FromQuery] UpdateUserDto updateUser)
    {
        var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "sid")!.Value);
        var result = await userService.UpdateUserAsync(updateUser, userId);
        return StatusCode(result.StatusCode, result);
    }
}