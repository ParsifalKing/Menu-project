using System.Net;
using Domain.DTOs.UserDTOs;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Services.FileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.UserService;

public class UserService(ILogger<UserService> logger, DataContext context, IFileService fileService) : IUserService
{
    #region GetUsersAsync

    public async Task<PagedResponse<List<GetUserDto>>> GetUsersAsync(UserFilter filter)
    {
        try
        {
            logger.LogInformation("Starting method GetUsersAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            var users = context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Username))
                users = users.Where(x => x.Username.ToLower().Contains(filter.Username.ToLower()));

            if (!string.IsNullOrEmpty(filter.Email))
                users = users.Where(x => x.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrEmpty(filter.Phone))
                users = users.Where(x => x.Phone.ToLower().Contains(filter.Phone.ToLower()));

            if (filter.Status != null)
            {
                if (filter.Status == Domain.Enums.Status.Active) users = users.Where(x => x.Status == "Active");
                if (filter.Status == Domain.Enums.Status.InActive) users = users.Where(x => x.Status == "InActive");
            }

            var response = await users.Select(x => new GetUserDto()
            {
                Email = x.Email,
                Phone = x.Phone,
                Username = x.Username,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Status = x.Status,
                PathPhoto = x.PathPhoto,
                Id = x.Id,
            }).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            var totalRecord = await users.CountAsync();

            logger.LogInformation("Finished method GetUsersAsync in time:{DateTime}", DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetUserDto>>(response, filter.PageNumber, filter.PageSize, totalRecord);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new PagedResponse<List<GetUserDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region GetUserByIdAsync

    public async Task<Response<GetUserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            logger.LogInformation("Starting method GetUserByIdAsync in time:{DateTime}", DateTimeOffset.UtcNow);
            var user = await context.Users.Select(x => new GetUserDto()
            {
                Email = x.Email,
                Phone = x.Phone,
                Username = x.Username,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Status = x.Status,
                PathPhoto = x.PathPhoto,
                Id = x.Id,
            }).FirstOrDefaultAsync(x => x.Id == userId);

            logger.LogInformation("Finished method GetUserByIdAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return user == null
                ? new Response<GetUserDto>(HttpStatusCode.BadRequest, $"User not found by ID:{userId}")
                : new Response<GetUserDto>(user);
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<GetUserDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion

    #region UpdateUserAsync

    public async Task<Response<string>> UpdateUserAsync(UpdateUserDto updateUser, Guid userId)
    {
        try
        {
            logger.LogInformation("Starting method UpdateUserAsync in time:{DateTime} ", DateTimeOffset.UtcNow);

            int existing = 0;
            if (updateUser.Status == Domain.Enums.Status.Active)
            {
                existing = await context.Users.Where(x => x.Id == userId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.Email, updateUser.Email)
                    .SetProperty(u => u.Phone, updateUser.Phone)
                    .SetProperty(u => u.Username, updateUser.Username)
                    .SetProperty(u => u.Status, "Active")
                );
            }
            else if (updateUser.Status == Domain.Enums.Status.InActive)
            {
                existing = await context.Users.Where(x => x.Id == userId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.Email, updateUser.Email)
                    .SetProperty(u => u.Phone, updateUser.Phone)
                    .SetProperty(u => u.Username, updateUser.Username)
                    .SetProperty(u => u.Status, "InActive")
                );
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                logger.LogWarning("Not found user by id:{userId} in time{Time}", userId, DateTime.UtcNow);
                return new Response<string>(HttpStatusCode.BadRequest, $"Not found user by id:{userId}");
            }
            if (updateUser.Photo != null)
            {
                if (user.PathPhoto != null) fileService.DeleteFile(user.PathPhoto);
                user.PathPhoto = await fileService.CreateFile(updateUser.Photo);
            }
            await context.SaveChangesAsync();


            logger.LogInformation("Finished method UpdateUserAsync in time:{DateTime} ", DateTimeOffset.UtcNow);
            return existing == 0
                ? new Response<string>(HttpStatusCode.BadRequest, "Invalid request ")
                : new Response<string>("Successfully updated ");
        }
        catch (Exception e)
        {
            logger.LogError("Exception {Exception}, time={DateTimeNow}", e.Message, DateTimeOffset.UtcNow);
            return new Response<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    #endregion
}