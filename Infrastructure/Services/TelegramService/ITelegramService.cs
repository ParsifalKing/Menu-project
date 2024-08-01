namespace Infrastructure.Services.TelegramService;

public interface ITelegramService
{
    public Task SendMessageToAdmin(string message);
}
