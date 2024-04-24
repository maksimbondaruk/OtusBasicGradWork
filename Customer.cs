using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static OtusBasicGradWork.Order;
using Newtonsoft.Json.Linq;

namespace OtusBasicGradWork
{    internal class Customer: IDisposable
    {
        private bool disposedValue;

        //public Dictionary<long, MapGenState> ChatDict { get; set; } = [];
        public Dictionary<long, Order> OrderDict { get; private set; }
        public async Task Process(ITelegramBotClient client, Update update, CancellationToken ct, User userData)
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
                        OrderDict[_orderIdx].Id = _orderIdx;
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
                    await GetImg(client, update, _order, ct, "A.jpg");
                    _order.State = Order.OrdState.LoadedA;
                    break;
                case Order.OrdState.LoadedA:
                    await GetImg(client, update, _order, ct, "B.jpg");
                    _order.State = Order.OrdState.LoadedB;
                    break;
                case Order.OrdState.LoadedB:
                    await GetVoteOrder(client, update, _order, ct, userData);
                    break;
                case Order.OrdState.BalanceLo:
                    await BalanceLo(client, update, _order, ct, userData);
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

        private async Task SetName(ITelegramBotClient client, Update update, Order order, CancellationToken ct)
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
        private async Task GetImg(ITelegramBotClient client, Update update, Order order, CancellationToken ct, string fileStoreName)
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
                        order.State--;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "отправьте сюда фото, а не сообщение",
                                                          cancellationToken: ct);
                        break;
                }
            }
            string dirName = Path.Combine(@"C:\", update.Message.Chat.Id.ToString(), "customer", order.Id.ToString());
            // если папка не существует
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            if (update.Message.Photo != null) 
            {
                var fileId = update.Message.Photo.Last().FileId;
                var fileInfo = await client.GetFileAsync(fileId, ct);
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested(); // генерируем исключение
                var _filePath = fileInfo.FilePath;

                string desinationFilePath = Path.Combine(dirName, _filePath, fileStoreName);
                await using FileStream fileStream = System.IO.File.OpenWrite(desinationFilePath);
                await client.DownloadFileAsync(filePath: _filePath, destination: fileStream);
            }
        }
        private async Task GetVoteOrder(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "На Вашем баллансе " + userData.Balance.ToString(),
                                  cancellationToken: ct);
            if ((order.VoteOrder == 0)||userData.RequestToChangeVoteOrder)
            {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                      text: "Введите сумму, которую готовы потратить на тест",
                      cancellationToken: ct);
                _ = int.TryParse(update.Message.Text, out var _orderValue);
                order.VoteOrder = _orderValue;
            }

            if (userData.Balance < order.VoteOrder) 
            {
                order.State = OrdState.BalanceLo;
                return;
            }
            order.State = OrdState.BalanceOk;
        }
        private async Task BalanceLo(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "На Вашем баллансе не хватает " + (userData.Balance - order.VoteOrder).ToString() +
                                  " баллов для этого заказа.\n Предпочитаете пополнить балланс или изменить сумму заказа?",
                                  cancellationToken: ct);
            var ordNameText = update.Message.Text;
            if (ordNameText != null)
            {
                switch (ordNameText)
                {
                    case "/addtobalance":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        userData.RequestToChangeVoteOrder = true;
                        order.State = Order.OrdState.LoadedB;
                        break;
                    case "/changevoteorder":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "возврат к предыдущему пункту",
                                                          cancellationToken: ct);
                        order.State--;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выберите предложенный вариант",
                                                          cancellationToken: ct);
                        break;
                }

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~Customer()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
