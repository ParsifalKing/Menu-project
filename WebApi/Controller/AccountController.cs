using Domain.Constants;
using Domain.DTOs.AccountDTOs;
using Infrastructure.Permissions;
using Infrastructure.Services.AccountService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService authService) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var response = await authService.Login(loginDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromQuery] RegisterDto registerDto)
    {
        var response = await authService.Register(registerDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("Change-Password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "sid")!.Value);
        var result = await authService.ChangePassword(changePasswordDto, userId!);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("Forgot-Password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await authService.ForgotPasswordCodeGenerator(forgotPasswordDto);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPut("Reset-Password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPassword(resetPasswordDto);
        return StatusCode(result.StatusCode, result);
    }


    [HttpDelete("Delete-Account")]
    [PermissionAuthorize(Permissions.User.Delete)]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "sid")!.Value);
        var result = await authService.DeleteAccount(userId!);
        return StatusCode(result.StatusCode, result);
    }
}