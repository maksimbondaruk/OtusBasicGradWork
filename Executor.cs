using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Requests;
using System.Text.RegularExpressions;

namespace OtusBasicGradWork
{
    internal class Executor
    {
        public async Task Process(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            var message = update.Message;
            if (message == null)
                return;
            if (message.Text !=null)
            {
                switch (message.Text)
                {
                    case "/customer":
                        await SendStateMachPic(client, update, ct);
                        break;
                    case "/tester":
                        break;
                    default:
                        await client.SendTextMessageAsync(
                            chatId: update.Message!.Chat.Id,
                            text: "попробуйте сделать выбор еще раз");
                        break;
                }
            }
        }

        async Task SendStateMachPic(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            var file = System.IO.File.ReadAllBytes($"1.png");
            await client.SendPhotoAsync(
                chatId: update.Message!.Chat.Id,
                photo: InputFile.FromStream(fileName:"котик.jpg", 
                                            stream: new MemoryStream(file)),
                caption: "Вот вам котик",
                cancellationToken: ct
                );
        }
    }
}
