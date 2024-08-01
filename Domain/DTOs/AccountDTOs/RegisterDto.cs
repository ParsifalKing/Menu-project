using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.AccountDTOs;

public class RegisterDto
{
    public required string UserName { get; set; }
    public required string Phone { get; set; }
    [DataType(DataType.EmailAddress)] public required string Email { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }
    [Compare("Password"), DataType(DataType.Password)]
    public required string ConfirmPassword { get; set; }
    public IFormFile? Photo { get; set; }
}