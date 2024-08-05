using Infrastructure.Data;
using Infrastructure.Permissions;
using Infrastructure.Seed;
using Infrastructure.Services.AccountService;
using Infrastructure.Services.CategoryService;
using Infrastructure.Services.DishCategoryService;
using Infrastructure.Services.DishIngredientService;
using Infrastructure.Services.DishService;
using Infrastructure.Services.DrinkService;
using Infrastructure.Services.EmailService;
using Infrastructure.Services.FileService;
using Infrastructure.Services.HashService;
using Infrastructure.Services.CheckIngredientsService;
using Infrastructure.Services.IngredientService;
using Infrastructure.Services.RoleService;
using Infrastructure.Services.UserRoleService;
using Infrastructure.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services.OrderService;
using Infrastructure.Services.OrderDetailService;
using Infrastructure.Services.NotificationService;
using Infrastructure.Services.TelegramService;
using Infrastructure.Services.DrinkIngredientService;

namespace WebApi.ExtensionMethods.RegisterService;

public static class RegisterService
{
    public static void AddRegisterService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(configure =>
            configure.UseNpgsql(configuration.GetConnectionString("Connection")));

        services.AddScoped<Seeder>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<ICheckIngredientsService, CheckIngredientsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IDishCategoryService, DishCategoryService>();
        services.AddScoped<IDishIngredientService, DishIngredientService>();
        services.AddScoped<IDrinkService, DrinkService>();
        services.AddScoped<IDrinkIngredientService, DrinkIngredientService>();
        services.AddScoped<IIngredientService, IngredientService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    }
}