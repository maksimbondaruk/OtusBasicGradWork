using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace OtusBasicGradWork
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("");
            var execImg = new Executor();
            var custImg = new Customer();
            var dictStatesOfUsers = new Dictionary<long, object>();
           
            var ro = new ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[] { },  
            };

            botClient.StartReceiving(updateHandler: Handler, pollingErrorHandler: ErrorHandler, receiverOptions: ro);
            Console.ReadLine();

            async Task Handler(ITelegramBotClient client, Update update, CancellationToken ct)
            {
                if (!dictStatesOfUsers.TryGetValue(update.Message.Chat.Id, out var state))
                {
                    dictStatesOfUsers.Add(update.Message.Chat.Id, ChatMode.Initial);
                }
                state = dictStatesOfUsers[update.Message.Chat.Id];

                if (update.Message.Text == "/main")
                {
                    dictStatesOfUsers[update.Message.Chat.Id] = ChatMode.Initial;
                    await SendMenu(client, update, ct);
                }
                else
                {
                    switch(state)
                    {
                        case ChatMode.Executor:
                            await execImg.Process(client, update, ct);
                            break;
                        case ChatMode.Customer:
                            await custImg.Process(client, update, ct);
                            break;
                        default:
                            switch (update.Message.Text)
                            {
                                case "/customer":
                                    await custImg.Process(client, update, ct);
                                    dictStatesOfUsers[update.Message.Chat.Id] = ChatMode.Customer;
                                    break;
                                case "/tester":
                                    await execImg.Process(client, update, ct);
                                    dictStatesOfUsers[update.Message.Chat.Id] = ChatMode.Executor;
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

            }
            Console.WriteLine("Hello, World!");
        }

        private static async Task SendMenu(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Выберите роль\n" +
                                              "/customer - получение фото\n" +
                                              "/tester - генерация координаты",
                                              cancellationToken: ct);
        }
    }
}
enum ChatMode
{
    Initial = 0,
    Executor = 1,
    Customer = 2,
}
