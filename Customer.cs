using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace OtusBasicGradWork
{

    /*class MapGenState
    {
        public double Lat {  get; set; }
        public double Long { get; set; }
        public int Balance { get; set; }
        public OrdState Mode { get; set; }
    }*/
    internal class Customer
    {
        //public Dictionary<long, MapGenState> ChatDict { get; set; } = [];
        public Dictionary<long, Order> OrderDict { get; private set; }
        public async Task Process(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            //Проверяем что был выбрана кнопка создать заказ    //!ChatDict.TryGetValue(update.Message.Chat.Id, out var state) - уже есть запись в словаре с таким Chat.Id
            long _orderIdx = 0;
            if (update.Message.Text == "/createorder") 
            {
                var tempOrder = new Order(update.Message.Chat.Id);
                for (var i = 0; i < 999; i++)
                {
                    if (OrderDict.TryAdd(update.Message.Chat.Id * 1000 + i, tempOrder))
                    {
                        _orderIdx = update.Message.Chat.Id * 1000 + i;
                        break;
                    }

                    
                    if (OrderDict.ContainsKey(update.Message.Chat.Id * 1000 + i) & (OrderDict[update.Message.Chat.Id * 1000 + i].State < Order.OrdState.Deleted))
                    {
                        continue;
                    }

                }    

            }
            
            if (_orderIdx == 0)
            {
                for (var i = 0; i < 999; i++)
                {
                    if (OrderDict.ContainsKey(update.Message.Chat.Id * 1000 + i) & (OrderDict[update.Message.Chat.Id * 1000 + i].State < Order.OrdState.Deleted))
                    {
                        _orderIdx = update.Message.Chat.Id * 1000 + i;
                        break;
                    }
                }
            }

            var state = OrderDict[_orderIdx].State;
            switch(state) 
            {
                case Order.OrdState.Initial:
                    await SendInitial(client, update, state, ct);
                    break;
                case Order.OrdState.Named:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.LoadedA:
                    await SendLong(client, update, state, ct);
                    state = Order.OrdState.Initial;
                    await SendInitial(client, update, state, ct);
                    break;
                case Order.OrdState.LoadedB:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.BalanceLo:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.BalanceOk:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.Running:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.Paused:
                    await SendLat(client, update, state, ct);
                    break;
                case Order.OrdState.Deleted:
                    await SendLat(client, update, state, ct);
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
            */
    }
}
