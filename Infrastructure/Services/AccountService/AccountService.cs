using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Domain.Constants;
using Domain.DTOs.AccountDTOs;
using Domain.DTOs.EmailDTOs;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.EmailService;
using Infrastructure.Services.FileService;
using Infrastructure.Services.HashService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Text;

namespace Infrastructure.Services.AccountService;

public class AccountService(DataContext context, ILogger<AccountService> logger, IEmailService emailService,
    IHashService hashService, IConfiguration configuration, IFileService fileService) : IAccountService
{
    #region Register

    public async Task<Response<string>> Register(RegisterDto model)
    {
        try
        {
            logger.LogInformation("Starting method Register in time {DateTime}", DateTimeOffset.UtcNow);
            var existing1 = await context.Users.AnyAsync(x => x.Username == model.UserName);
            if (existing1)
            {
                logger.LogWarning("Alredy exists this username:{Username}, in time {DateTime}",
                    model.UserName, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest,
                    $"User already exists by this username={model.UserName}");
            }
            var existing2 = await context.Users.AnyAsync(x => x.Email == model.Email || x.Phone == model.Phone);
            if (existing2)
            {
                logger.LogWarning("Alredy exists this email:{Email} or this phonenumber:{Phone}, in time {DateTime}",
                    model.Email, model.Phone, DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest,
                    $"User already exists by this email={model.Email} or phonenumber={model.Phone}");
            }

            var newUser = new User()
            {
                Username = model.UserName,
                Email = model.Email,
                Phone = model.Phone,
                Status = "Active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Password = hashService.ConvertToHash(model.Password),
            };
            if (model.Photo != null) newUser.PathPhoto = await fileService.CreateFile(model.Photo);

            var result = await context.Users.AddAsync(newUser);
            await context.SaveChangesAsync();

            var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.User);
            if (role == null)
            {
                logger.LogWarning("Not found role by name:{roleId} in time:{DateTime}", Roles.User, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not found role by name:{Roles.User}");
            }

            var newUserRole = new UserRole()
            {
                UserId = result.Entity.Id,
                RoleId = role!.Id,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.UserRoles.AddAsync(newUserRole);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method Register in time {DateTime}", DateTimeOffset.UtcNow);
            return new Response<string>($"Successfully created new User by Id={newUser.Id}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception} in time:{DateTime} ", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region Login

    public async Task<Response<string>> Login(LoginDto model)
    {
        try
        {
            logger.LogInformation("Starting method Login in time {DateTime}", DateTimeOffset.UtcNow);

            var existing = await context.Users.FirstOrDefaultAsync(x =>
                x.Username == model.UserName && x.Password == hashService.ConvertToHash(model.Password));
            if (existing is null)
            {
                logger.LogWarning("Username or password incorrect,time={DateTimeNow} ", DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, "Username or password incorrect");
            }

            logger.LogInformation("Finished method Login in time {DateTime}", DateTimeOffset.UtcNow);
            return new Response<string>(await GenerateJwtToken(existing));
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception} in time:{DateTime} ", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GenerateJwtToken

    private async Task<string> GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]!);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new("Phone", user.Phone),
            new("UserStatus", user.Status.ToString())
        };
        //add roles

        var roles = await context.UserRoles.Where(x => x.UserId == user.Id).Include(x => x.Role)
            .Select(x => x.Role).ToListAsync();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role!.Name));
            var roleClaims = await context.RoleClaims.Where(x => x.RoleId == role.Id).ToListAsync();
            foreach (var roleClaim in roleClaims)
            {
                claims.Add(new Claim("Permissions", roleClaim.ClaimValue));
            }
        }

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var securityTokenHandler = new JwtSecurityTokenHandler();
        var tokenString = securityTokenHandler.WriteToken(token);
        return tokenString;
    }

    #endregion


    #region ForgotPasswordCodeGenerator

    public async Task<Response<string>> ForgotPasswordCodeGenerator(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            logger.LogInformation("Starting method ForgotPasswordCodeGenerator in time {DateTime}", DateTimeOffset.UtcNow);
            var existing = await context.Users.FirstOrDefaultAsync(x => x.Email == forgotPasswordDto.Email);
            if (existing is null)
            {
                logger.LogWarning("Not found user with email {Email} ,time={DateTimeNow} ", forgotPasswordDto.Email,
                    DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest,
                    $"Not Found User with email {forgotPasswordDto.Email}");
            }

            var random = new Random();
            existing.Code = random.Next(1000, 9999).ToString();
            existing.CodeTime = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync();

            await emailService.SendEmail(new EmailMessageDto(new[] { forgotPasswordDto.Email }, "Reset password",
                $"<h1>{existing.Code}</h1>"), TextFormat.Html);

            logger.LogInformation("Finished method ForgotPasswordCodeGenerator in time {DateTime}", DateTimeOffset.UtcNow);
            return new Response<string>("Success");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception} in time:{DateTime} ", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion



    #region ResetPassword

    public async Task<Response<string>> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            logger.LogInformation("Starting method ResetPassword in time {DateTime}", DateTimeOffset.UtcNow);
            var existing = await context.Users.FirstOrDefaultAsync(x => x.Email == resetPasswordDto.Email);
            if (existing is null)
            {
                logger.LogWarning("Not found user with {Email},time={DateTimeNow}", resetPasswordDto.Email,
                    DateTimeOffset.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not Found User with {resetPasswordDto.Email}");
            }
            if (resetPasswordDto.Code != existing.Code)
            {
                logger.LogWarning("Code is not recognized in time{DateTime}", DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Code:{resetPasswordDto.Code} is not recognized");
            }

            var timeCode = DateTimeOffset.UtcNow - existing.CodeTime;
            var year = DateTimeOffset.UtcNow.Year - existing.CodeTime.Year;

            if (timeCode.Days == 0 && timeCode.Hours == 0 && timeCode.Minutes <= 3 && year == 0)
            {
                existing.Password = hashService.ConvertToHash(resetPasswordDto.Password);
                await context.SaveChangesAsync();

                logger.LogInformation("Finished method ResetPassword in time {DateTime}", DateTimeOffset.UtcNow);
                return new Response<string>("Success");
            }

            logger.LogWarning("Failed {Code},time={DateTimeNow}", resetPasswordDto.Code, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.BadRequest, $"Failed {resetPasswordDto.Code}");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception} in time:{DateTime} ", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion


    #region ChangePassword

    public async Task<Response<string>> ChangePassword(ChangePasswordDto passwordDto, Guid userId)
    {
        try
        {
            logger.LogInformation("Starting method ChangePassword in time {DateTime}", DateTimeOffset.UtcNow);

            var existing = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (existing is null)
            {
                logger.LogWarning("User not found,in time {DateTime}", DateTimeOffset.Now);
                return new Response<string>(HttpStatusCode.BadRequest, "User not found");
            }

            if (existing.Password != hashService.ConvertToHash(passwordDto.OldPassword))
            {
                logger.LogWarning("Failed old Password,in time {DateTime}", DateTimeOffset.Now);
                return new Response<string>(HttpStatusCode.BadRequest, "Failed old Password");
            }

            existing.Password = hashService.ConvertToHash(passwordDto.Password);
            await context.SaveChangesAsync();

            logger.LogInformation("Finished method ChangePassword in time {DateTime}", DateTimeOffset.UtcNow);
            return new Response<string>("Successfully changed password");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception} in time:{DateTime} ", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region DeleteAccount

    public async Task<Response<string>> DeleteAccount(Guid userId)
    {
        try
        {
            logger.LogInformation("Starting method DeleteAccount in time:{DateTime} ", DateTimeOffset.UtcNow);

            var user = await context.Users.Where(x => x.Id == userId).ExecuteDeleteAsync();
            logger.LogInformation("Finished method DeleteAccount in time:{DateTime} ", DateTimeOffset.UtcNow);

            return user == 0
                ? new Response<string>(HttpStatusCode.BadRequest, $"User not found by Id:{userId}")
                : new Response<string>("Successfully deleted account");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion
}