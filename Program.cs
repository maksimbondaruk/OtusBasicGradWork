using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace OtusBasicGradWork
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Получаем значение токена из переменной среды
            //string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Machine)!;

            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("Telegram bot token is not set in the environment variables.");
                return;
            }

            var botClient = new TelegramBotClient(botToken);
            var _executor = new Executor();
            var _customer = new Customer();
            var _customerTestBalance = 1000;
            var dictStatesOfUsers = new Dictionary<long, User>();

            CancellationTokenSource ct = new CancellationTokenSource();
            CancellationToken token = ct.Token;

                        var ro = new ReceiverOptions
                        {
                            AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[] { },  
                        };

                        botClient.StartReceiving(updateHandler: Handler, pollingErrorHandler: ErrorHandler, receiverOptions: ro);

            async Task Handler(ITelegramBotClient client, Update update, CancellationToken ct)
            {
                if (!dictStatesOfUsers.TryGetValue(update.Message.Chat.Id, out var userData))
                {
                    dictStatesOfUsers.Add(update.Message.Chat.Id, new User(_customerTestBalance, ChatMode.Initial));
                    userData = dictStatesOfUsers[update.Message.Chat.Id];
                }
                //state.ChatMode = dictStatesOfUsers[update.Message.Chat.Id].ChatMode;

                if (update.Message.Text == "/main")
                {
                    userData.ChatMode = ChatMode.Initial;
                    await SendMenu(client, update, ct);
                }
                else
                {
                    switch(userData.ChatMode)
                    {
                        case ChatMode.Customer:
                            await _customer.Process(client, update, ct, userData);
                            break;
                        case ChatMode.Executor:
                            await _executor.Process(client, update, ct);
                            break;
                        default:
                            switch (update.Message.Text)
                            {
                                case "/customer":
                                    await _customer.Process(client, update, ct, userData);
                                    dictStatesOfUsers[update.Message.Chat.Id].ChatMode = ChatMode.Customer;
                                    break;
                                case "/tester":
                                    await _executor.Process(client, update, ct);
                                    dictStatesOfUsers[update.Message.Chat.Id].ChatMode = ChatMode.Executor;
                                    break;
                                default:
                                    await SendMenu(client, update, ct);
                                    break;
                            }
                            break; 
                    }
                }
               // var chat = update.Message.Chat;
               /*await client.SendTextMessageAsync(chatId: chat.Id, 
                    text: $"Привет, {chat.FirstName}",
                    cancellationToken: ct);*/
            }

            async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken ct)
            {
                await Console.Out.WriteLineAsync("Свалились в ErrorHandler");
            }

            Console.WriteLine("Bot started. Press any key to stop...");
            Console.ReadLine();
        }

        private static async Task SendMenu(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Выберите роль\n" +
                                              "/customer - заказчик\n" +
                                              "/tester - тестировщик",
                                              cancellationToken: ct);
        }
    }
    enum ChatMode
    {
        Initial = 0,
        Executor = 1,
        Customer = 2,
    }
}
