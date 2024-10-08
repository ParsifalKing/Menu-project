
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace Infrastructure.Services.TelegramService;
public class TelegramService : ITelegramService
{
    private readonly string _adminChatId;
    private readonly TelegramBotClient _botClient;

    public TelegramService(string botToken, string adminChatId)
    {
        _adminChatId = adminChatId;
        _botClient = new TelegramBotClient(botToken);
    }

    public async Task SendMessageToAdmin(string message)
    {
        await _botClient.SendTextMessageAsync(_adminChatId, message);
    }

}
