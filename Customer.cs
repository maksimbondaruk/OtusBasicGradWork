using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static OtusBasicGradWork.Order;

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
                        OrderDict[_orderIdx].State = Order.OrdState.Initial;
                        break;
                    }
                }
            }
            
            if (_orderIdx == 0)
            {
                for (var i = 0; i < 999; i++)
                {
                    if (OrderDict.ContainsKey(update.Message.Chat.Id * 1000 + i) & (OrderDict[update.Message.Chat.Id * 1000 + i].State < Order.OrdState.ToDelete))
                    {
                        _orderIdx = update.Message.Chat.Id * 1000 + i;
                        break;
                    }
                }
            }

            //var state = OrderDict[_orderIdx].State;
            var _order = OrderDict[_orderIdx];
            switch (_order.State) 
            {
                case Order.OrdState.Initial:
                    await SetName(client, update, _order, ct);
                    break;
                case Order.OrdState.Named:
                    await GetA(client, update, _order, ct);
                    break;
                case Order.OrdState.LoadedA:
                    await SendLong(client, update, _order, ct);
                    state = Order.OrdState.Initial;
                    await SetName(client, update, _order, ct);
                    break;
                case Order.OrdState.LoadedB:
                    await SendLat(client, update, _order, ct);
                    break;
                case Order.OrdState.BalanceLo:
                    await SendLat(client, update, _order, ct);
                    break;
                case Order.OrdState.BalanceOk:
                    await SendLat(client, update, _order, ct);
                    break;
                case Order.OrdState.Running:
                    await SendLat(client, update, _order, ct);
                    break;
                case Order.OrdState.Paused:
                    await SendLat(client, update, _order, ct);
                    break;
                case Order.OrdState.ToDelete:
                    await SendLat(client, update, _order, ct);
                    break;
            }
        }

        private static async Task SetName(ITelegramBotClient client, Update update, Order order, CancellationToken ct)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Введите название теста",
                                              cancellationToken: ct);
            //Тут нужна функция показать кнопки
            var ordNameText = update.Message.Text;
            if (ordNameText != null)
            {
                if (ordNameText == "/maincustomer")
                {
                    await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "Сброс заказа, возврат в меню заказчика",
                                  cancellationToken: ct);
                    order.State = Order.OrdState.ToDelete;
                }
                else
                {
                    order.Name = ordNameText;
                    order.State = Order.OrdState.Named;
                }
            }
        }
        private static async Task GetA(ITelegramBotClient client, Update update, Order order, CancellationToken ct)
        {
            var ordNameText = update.Message.Text;
            if (ordNameText != null)
            {
                switch (ordNameText)
                {
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    case "/back":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "возврат к предыдущему пункту",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.Named;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "отправьте сюда фото, а не сообщение",
                                                          cancellationToken: ct);
                        break;
                }
            }
            //Сюда надо вставить:
            //создание папки клиента
            //папки заказа
            //и туда приемку фото
        }
    }
}
