using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace OtusBasicGradWork
{
    enum OrdState
    {
        Initial = 0,
        SetLat = 1, 
        SetLon = 2,
        /*Name = 1,
        A = 2,
        B = 3,
        BalLo = 4,
        BalOk = 5,
        Running = 6,
        Pause = 7,
        Delete = 8*/
    }
    class MapGenState
    {
        public double Lat {  get; set; }
        public double Long { get; set; }
        public int Balance { get; set; }
        public OrdState Mode { get; set; }
    }
    internal class Customer
    {
        public Dictionary<long, MapGenState> ChatDict { get; set; } = [];
        public async Task Process(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            if (!ChatDict.TryGetValue(update.Message.Chat.Id, out var state)) 
            { 
                ChatDict.Add(update.Message.Chat.Id, new MapGenState()); 
            }
            state = ChatDict[update.Message.Chat.Id];

            switch(state.Mode) 
            {
                case OrdState.Initial:
                    await SendInitial(client, update, state, ct);
                    break;
                case OrdState.SetLat:
                    await SendLat(client, update, state, ct);
                    break;
                case OrdState.SetLon:
                    await SendLong(client, update, state, ct);
                    state.Mode = OrdState.Initial;
                    await SendInitial(client, update, state, ct);
                    break;
            }
        }

        private static async Task SendLong(ITelegramBotClient client, Update update, MapGenState? state, CancellationToken ct)
        {
            var lonText = update.Message.Text;
            if (lonText == null || !double.TryParse(lonText, out var lon))
            {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                  text: "Введите долготу корректно",
                                                  cancellationToken: ct);
            }
            else
                state.Long = lon;
            await client.SendLocationAsync(chatId: update.Message.Chat.Id,
                                           latitude: state.Lat,
                                           longitude: state.Long,
                                           cancellationToken: ct);

            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Вот Ваша точка",
                                              cancellationToken: ct);
        }

        private static async Task SendLat(ITelegramBotClient client, Update update, MapGenState? state, CancellationToken ct)
        {
            var latText = update.Message.Text;
            if (latText == null || !double.TryParse(latText, out var lat))
            {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                  text: "Введите широту корректно",
                                                  cancellationToken: ct);
            }
            else
                state.Lat = lat;
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Введите долготу",
                                              cancellationToken: ct);
            state.Mode = OrdState.SetLon;
        }

        private static async Task SendInitial(ITelegramBotClient client, Update update, MapGenState? state, CancellationToken ct)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Введите широту",
                                              cancellationToken: ct);
            state.Mode = OrdState.SetLat;
        }
    }
}
