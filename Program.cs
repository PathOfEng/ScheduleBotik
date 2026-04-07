using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

internal class Program
{
    static string[] weekSchedule = new string[6];
    //Всё, что делает клиент - асинхронное
    async static void GetHelpMessage(ITelegramBotClient client, Update update)
    {
        string info = "Команда /CrnWeekSchedule - предоставляет расписание на всю неделю\n" +
                        "Команда /NextLesson - предоставляет информацию о ближайшей паре\n" +
                        "Команда /help - список всех команд";
        await client.SendMessage(update.Message.Chat.Id, info, replyMarkup: ReplyMarkups);
    }

    async static void GetMessage(ITelegramBotClient client, Update update, string message)
    {
        await client.SendMessage(update.Message.Chat.Id, message);
    }
    static ReplyKeyboardMarkup ReplyMarkups = new ReplyKeyboardMarkup(new[]
    {

        new KeyboardButton[]
        {
            new KeyboardButton("Ближайшая пара")
        },
        new KeyboardButton[]
        {
            new KeyboardButton("Расписание на сегодня"),
            new KeyboardButton("Расписание на завтра")
        },
        new KeyboardButton[]
        {
            new KeyboardButton("Расписание на неделю")
        },
        new KeyboardButton[]
        {
            new KeyboardButton("Обновить расписание")
        }
    })
    {
        ResizeKeyboard = true //Чтобы кнопки не были большими
    };
    private static void Main(string[] args)
    {
        Host ScheduleBot = new Host("8656287848:AAG_X9aRibkczuOqSS61C5x0YfzWxNJwg3w");
        ScheduleBot.Start();
        ScheduleBot.OnMessage += DelegateMessage;
        Console.ReadLine();
    }

    //private static async void GetWeekSchedule(string group, int kurs, DateTime day)
    //{
    //    string[] schedule = new string[6];
    //    DateTime now = DateTime.Now;
    //    // Вычисляем количество дней, прошедших с понедельника
    //    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
    //    if (diff == 0)
    //    {
    //        Methods.DownloadHtml();
    //        now.AddDays(1);
    //        for (int i = 0; i < 6; i++)
    //        {
    //            GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, now.AddDays(i)));
    //        }
    //    }
    //    else
    //    {
    //        DateTime startOfWeek = now.AddDays(-1 * diff);
    //        for (int i = 0; i < 6; i++)
    //        {
    //            GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, startOfWeek.AddDays(i)));
    //        }
    //    }
    //}

    private static async void DelegateMessage(ITelegramBotClient client, Update update)
    {
        try
        {
            switch (update.Message?.Text)
            {
                case "/start":
                    GetHelpMessage(client, update);
                    break;
                case "/help":
                    GetHelpMessage(client, update);
                    break;
                case "/download":
                    await Methods.DownloadHtml();
                    break;
                case "Расписание на неделю":
                    DateTime now = DateTime.Now;
                    // Вычисляем количество дней, прошедших с понедельника
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    //Расписание на сайте выкладывается в двух экземплярах и если
                    //день недели воскресенье, то на сайте еще может остаться старое расписание,
                    //но также появится новое. Для этого мы и ищем разницу в днях.
                    //Для воскресенья
                    if (diff == 0)
                    {
                        await Methods.DownloadHtml();
                        now.AddDays(1);
                        for (int i = 0; i < 6; i++)
                        {
                            GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, now.AddDays(i)));
                        }
                    }
                    //Для любого друго дня недели - показать расписание на текущую неделю
                    else
                    {
                        DateTime startOfWeek = now.AddDays(-1 * diff);
                        for (int i = 0; i < 6; i++)
                        {
                            GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, startOfWeek.AddDays(i)));
                        }
                    }
                    break;

                case "Расписание на сегодня":
                    GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, DateTime.Today));
                    break;
                case "Расписание на завтра":
                    diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    if (diff == 1)
                    {
                        GetMessage(client, update, "Завтра пар нет. Завтра воскресенье, время отдыхать");
                        return;
                    }
                    else if (diff == 0)
                    {
                        await Methods.DownloadHtml();
                    }
                    GetMessage(client, update, Methods.GetSchedulePerDay("РИС-25-2", 1, DateTime.Today.AddDays(1)));
                    break;

                case "Ближайшая пара":
                    Methods.GetNearlyLesson();
                    break;
                case "Обновить расписание":
                    await Methods.DownloadHtml();
                    GetMessage(client, update, "Расписание успешно загружено");
                    break;
                default:
                    return;

            }
        }
        catch(Exception ex)
        {
            GetMessage(client, update, ex.Message);
        }
        



        if (update.Message?.Text == "/start" || update.Message?.Text == "/help")
        {

        }

    }


    

}