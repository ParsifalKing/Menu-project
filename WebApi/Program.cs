using Domain.DTOs.EmailDTOs;
using Infrastructure.Data;
using Infrastructure.Seed;
using Infrastructure.Services.TelegramService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApi.ExtensionMethods.AccountConfig;
using WebApi.ExtensionMethods.RegisterService;
using WebApi.ExtensionMethods.SwaggerConfig;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();


builder.Services.AddSingleton<ITelegramService>(provider =>
{
    var botToken = builder.Configuration["Telegram:BotToken"];
    var adminChatId = builder.Configuration["Telegram:AdminChatId"];
    if (string.IsNullOrEmpty(botToken))
    {
        throw new ArgumentNullException(nameof(botToken), "Bot token cannot be null or empty.");
    }
    if (string.IsNullOrEmpty(adminChatId))
    {
        throw new ArgumentNullException(nameof(adminChatId), "Admin chat ID cannot be null or empty.");
    }
    return new TelegramService(botToken, adminChatId);
});


builder.Services.AddSingleton(emailConfig!);

builder.Services.AddRegisterService(builder.Configuration);

builder.Services.SwaggerService();

builder.Services.AddAuthConfigureService(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });

var app = builder.Build();

app.UseCors(
    build => build.WithOrigins("http://localhost:3000", "https://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod());

try
{
    var serviceProvider = app.Services.CreateScope().ServiceProvider;
    var dataContext = serviceProvider.GetRequiredService<DataContext>();
    await dataContext.Database.MigrateAsync();

    var seeder = serviceProvider.GetRequiredService<Seeder>();
    await seeder.Initial();

}
catch (Exception)
{

}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
