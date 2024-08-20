using Domain.Constants;
using Domain.DTOs.RolePermissionDTOs;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Infrastructure.Services.HashService;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Seed;

public class Seeder(DataContext context, IHashService hashService)
{
    public async Task Initial()
    {
        await SeedRole();
        await DefaultUsers();
        await SeedClaimsForSuperAdmin();
        await AddAdminPermissions();
        await AddUserPermissions();
        await SeedBlockOrderControl();
    }


    #region SeedRole

    private async Task SeedRole()
    {
        try
        {
            var newRoles = new List<Role>()
            {
                new()
                {
                    Name = Roles.SuperAdmin,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Name = Roles.Admin,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Name = Roles.User,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
            };

            var existing = await context.Roles.ToListAsync();
            foreach (var role in newRoles)
            {
                if (existing.Exists(e => e.Name == role.Name) == false)
                {
                    await context.Roles.AddAsync(role);
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            //ignored
        }
    }

    #endregion

    #region SeedBlockOrderControl

    public async Task<string> SeedBlockOrderControl()
    {
        try
        {
            var blockOrderingControl = await context.BlockOrderControl.AnyAsync();
            if (blockOrderingControl == false)
            {
                var blockOrderControl = new BlockOrderControl()
                {
                    Id = 1,
                    IsBlocked = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
                await context.BlockOrderControl.AddAsync(blockOrderControl);
                await context.SaveChangesAsync();
                return "BlockOrderControl added succesfully";
            }
            return "BlockOrderControl already exist!!!";

        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    #endregion


    #region DefaultUsers

    private async Task DefaultUsers()
    {
        try
        {
            //Super-Admin
            var existingSuperAdmin = await context.Users.FirstOrDefaultAsync(x => x.Username == "SuperAdmin");
            if (existingSuperAdmin is null)
            {
                var superAdmin = new User()
                {
                    Email = "kkirapm@gmail.com ",
                    Phone = "918417869",
                    Username = "SuperAdmin",
                    Status = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Password = hashService.ConvertToHash("12345")
                };
                await context.Users.AddAsync(superAdmin);
                await context.SaveChangesAsync();

                var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Username == "SuperAdmin");
                var existingRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.SuperAdmin);
                if (existingUser is not null && existingRole is not null)
                {
                    var userRole = new UserRole()
                    {
                        RoleId = existingRole.Id,
                        UserId = existingUser.Id,
                        Role = existingRole,
                        User = existingUser,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await context.UserRoles.AddAsync(userRole);
                    await context.SaveChangesAsync();
                }
            }


            //Admin
            var existingAdmin = await context.Users.FirstOrDefaultAsync(x => x.Username == "Admin");
            if (existingAdmin is null)
            {
                var admin = new User()
                {
                    Email = "ymmumenu@gmail.com",
                    Phone = "017860010",
                    Username = "Admin",
                    Status = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Password = hashService.ConvertToHash("1234")
                };
                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();

                var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Username == "Admin");
                var existingRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.Admin);
                if (existingUser is not null && existingRole is not null)
                {
                    var userRole = new UserRole()
                    {
                        RoleId = existingRole.Id,
                        UserId = existingUser.Id,
                        Role = existingRole,
                        User = existingUser,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await context.UserRoles.AddAsync(userRole);
                    await context.SaveChangesAsync();
                }

            }

            //User
            var user = await context.Users.FirstOrDefaultAsync(x => x.Username == "Eraj");
            if (user is null)
            {
                var newUser = new User()
                {
                    Email = "erajismonov01@gmail.com",
                    Phone = "905855445",
                    Username = "Eraj",
                    Status = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Password = hashService.ConvertToHash("123")
                };
                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Username == "Eraj");
                var existingRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.User);
                if (existingUser is not null && existingRole is not null)
                {
                    var userRole = new UserRole()
                    {
                        RoleId = existingRole.Id,
                        UserId = existingUser.Id,
                        Role = existingRole,
                        User = existingUser,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await context.UserRoles.AddAsync(userRole);
                    await context.SaveChangesAsync();
                }

            }

        }
        catch (Exception)
        {
            //ignored;
        }
    }

    #endregion


    #region SeedClaimsForSuperAdmin

    private async Task SeedClaimsForSuperAdmin()
    {
        try
        {
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.SuperAdmin);
            if (superAdminRole == null) return;
            var roleClaims = new List<RoleClaimsDto>();
            roleClaims.GetPermissions(typeof(Domain.Constants.Permissions));
            var existingClaims = await context.RoleClaims.Where(x => x.RoleId == superAdminRole.Id).ToListAsync();
            foreach (var claim in roleClaims)
            {
                if (existingClaims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value) == false)
                    await context.AddPermissionClaim(superAdminRole, claim.Value);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    #endregion

    #region AddAdminPermissions

    private async Task AddAdminPermissions()
    {
        //add claims
        var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.Admin);
        if (adminRole == null) return;
        var userClaims = new List<RoleClaimsDto>()
        {
            new("Permissions", Domain.Constants.Permissions.Category.View),
            new("Permissions", Domain.Constants.Permissions.Category.Create),
            new("Permissions", Domain.Constants.Permissions.Category.Edit),

            new("Permissions", Domain.Constants.Permissions.Dish.View),
            new("Permissions", Domain.Constants.Permissions.Dish.Create),
            new("Permissions", Domain.Constants.Permissions.Dish.Edit),

            new("Permissions", Domain.Constants.Permissions.DishCategory.View),
            new("Permissions", Domain.Constants.Permissions.DishCategory.Create),
            new("Permissions", Domain.Constants.Permissions.DishCategory.Edit),

            new("Permissions", Domain.Constants.Permissions.DishIngredient.View),
            new("Permissions", Domain.Constants.Permissions.DishIngredient.Create),
            new("Permissions", Domain.Constants.Permissions.DishIngredient.Edit),

            new("Permissions", Domain.Constants.Permissions.DrinkIngredient.View),
            new("Permissions", Domain.Constants.Permissions.DrinkIngredient.Create),
            new("Permissions", Domain.Constants.Permissions.DrinkIngredient.Edit),

            new("Permissions", Domain.Constants.Permissions.Drink.View),
            new("Permissions", Domain.Constants.Permissions.Drink.Create),
            new("Permissions", Domain.Constants.Permissions.Drink.Edit),

            new("Permissions", Domain.Constants.Permissions.Ingredient.View),
            new("Permissions", Domain.Constants.Permissions.Ingredient.Create),
            new("Permissions", Domain.Constants.Permissions.Ingredient.Edit),

            new("Permissions", Domain.Constants.Permissions.Order.View),
            new("Permissions", Domain.Constants.Permissions.Order.Create),
            new("Permissions", Domain.Constants.Permissions.Order.Edit),
            new("Permissions", Domain.Constants.Permissions.Order.Delete),

            new("Permissions", Domain.Constants.Permissions.OrderDetail.View),
            new("Permissions", Domain.Constants.Permissions.OrderDetail.Create),
            new("Permissions", Domain.Constants.Permissions.OrderDetail.Edit),
            new("Permissions", Domain.Constants.Permissions.OrderDetail.Delete),

            new("Permissions", Domain.Constants.Permissions.Notification.View),
            new("Permissions", Domain.Constants.Permissions.Notification.Create),

            new("Permissions", Domain.Constants.Permissions.Role.View),

            new("Permissions", Domain.Constants.Permissions.User.View),
            new("Permissions", Domain.Constants.Permissions.User.Create),
            new("Permissions", Domain.Constants.Permissions.User.Edit),

            new("Permissions", Domain.Constants.Permissions.UserRole.View),
        };

        var existingClaim = await context.RoleClaims.Where(x => x.RoleId == adminRole.Id).ToListAsync();
        foreach (var claim in userClaims)
        {
            if (!existingClaim.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                await context.AddPermissionClaim(adminRole, claim.Value);
            }
        }
    }

    #endregion

    #region AddUserPermissions

    private async Task AddUserPermissions()
    {
        //add claims
        var userRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == Roles.User);
        if (userRole == null) return;
        var userClaims = new List<RoleClaimsDto>()
        {
            new("Permissions", Domain.Constants.Permissions.Category.View),
            new("Permissions", Domain.Constants.Permissions.Dish.View),
            new("Permissions", Domain.Constants.Permissions.DishCategory.View),
            new("Permissions", Domain.Constants.Permissions.DishIngredient.View),
            new("Permissions", Domain.Constants.Permissions.Drink.View),
            new("Permissions", Domain.Constants.Permissions.Role.View),
            new("Permissions", Domain.Constants.Permissions.User.Edit),
            new("Permissions", Domain.Constants.Permissions.Order.Create),
            new("Permissions", Domain.Constants.Permissions.Order.View),
        };

        var existingClaim = await context.RoleClaims.Where(x => x.RoleId == userRole.Id).ToListAsync();
        foreach (var claim in userClaims)
        {
            if (!existingClaim.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                await context.AddPermissionClaim(userRole, claim.Value);
            }
        }
    }

    #endregion


}




