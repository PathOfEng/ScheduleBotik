using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

public class Host
{
    public Action<ITelegramBotClient, Update>? OnMessage;
    TelegramBotClient tgBot;
    public Host(string token)
    {
        tgBot = new TelegramBotClient(token);
    }

    public void Start()
    {
        tgBot.StartReceiving(UpdateHandler, ErrorHandler);
        Console.WriteLine("Бот запущен");
    }

    private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        Console.WriteLine("Ошибка " + exception.Message);
        //throw new NotImplementedException();
        await Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
    {
        //throw new NotImplementedException();
        Console.WriteLine($"Пришло сообщение: {update.Message?.Text ?? "[не текст]"}");
        OnMessage?.Invoke(client, update);
        await Task.CompletedTask;
    }
}