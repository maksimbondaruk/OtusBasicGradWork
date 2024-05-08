using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static OtusBasicGradWork.Order;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

namespace OtusBasicGradWork
{    internal class Customer: IDisposable
    {
        private bool disposedValue;
        public Dictionary<long, Order> OrderDict { get; private set; } = new Dictionary<long, Order>();
        public async Task Process(ITelegramBotClient client, Update update, CancellationToken ct, User userData)
        {
            if (update.Message.Text == "/customer")
            {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                  text: "Хотите создать заказ /createorder \n " +
                                                  "или посмотреть список своих заказов /showorderlist",
                                                  cancellationToken: ct);
                return;
            }
            //Проверяем что был выбрана кнопка создать заказ
            long _orderIdx = 0;
            if (update.Message.Text == "/createorder") 
            {
                var tempOrder = new Order(update.Message.Chat.Id);
                for (var i = 1; i < 1000; i++)
                {
                    if (OrderDict.TryAdd(update.Message.Chat.Id * 1000 + i, tempOrder))
                    {
                        await Console.Out.WriteLineAsync("Добавил новый заказ в словарь");
                        _orderIdx = update.Message.Chat.Id * 1000 + i;
                        OrderDict[_orderIdx].State = Order.OrdState.Initial;
                        OrderDict[_orderIdx].Id = _orderIdx;
                        break;
                    }
                }
            }
            else
            {
                for (var i = 1; i < 1000; i++)
                {
                    if (OrderDict.ContainsKey(update.Message.Chat.Id * 1000 + i) & (OrderDict[update.Message.Chat.Id * 1000 + i].State < Order.OrdState.ToDelete))
                    {
                        await Console.Out.WriteLineAsync("Нашел заказ в словаре");
                        _orderIdx = update.Message.Chat.Id * 1000 + i;
                        break;
                    }
                    return;
                }
            }
            //if (_orderIdx == 0) return;

            var _order = OrderDict[_orderIdx];
            var _lowBalanceState = userData.Balance <= (OrderDict[_orderIdx].VoteOrder - OrderDict[_orderIdx].VoteActual)*userData.BalToVoteKoef;

            //StateConditionTable
            switch (_order.State) 
            {
                case Order.OrdState.Initial:
                    await SetName(client, update, _order, ct);
                    await RequestImgText(client, update, _order, OrdState.Named, "Пришли фото А", ct);
                    break;
                case Order.OrdState.Named:
                    await GetImg(client, update, _order, ct, "A.jpg");
                    _order.State = Order.OrdState.LoadedA;
                    await RequestImgText(client, update, _order, OrdState.LoadedA, "Пришли фото Б", ct);
                    break;
                case Order.OrdState.LoadedA:
                    await GetImg(client, update, _order, ct, "B.jpg");
                    _order.State = Order.OrdState.LoadedB;
                    await GetVoteOrder(client, update, _order, ct, userData, _lowBalanceState);
                    break;
                case Order.OrdState.LoadedB:
                    //await GetVoteOrder(client, update, _order, ct, userData, _lowBalanceState);
                    break;
                case Order.OrdState.BalanceLo:
                    await BalanceLo(client, update, _order, ct, userData);
                    break;
                case Order.OrdState.BalanceOk:
                    await BalanceOk(client, update, _order, ct, userData);
                    break;
                case Order.OrdState.Running:
                    await Running(client, update, _order, ct, userData, _lowBalanceState);
                    break;
                case Order.OrdState.Paused:
                    await Paused(client, update, _order, ct, userData, _lowBalanceState);
                    break;
                case Order.OrdState.ToDelete:
                    await DeleteOrder(client, update, _orderIdx, _order, ct);
                    break;
            }
        }

        private static async Task RequestImgText(ITelegramBotClient client, Update update, Order _order, OrdState orderstate, string usermessage, CancellationToken ct)
        {
            if (_order.State == orderstate)
            {
                await Console.Out.WriteLineAsync("Пробуем запросить изображение: " + usermessage);
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                  text: usermessage,
                                                  cancellationToken: ct);
            }

            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                      text: "Это просто кнопки для меню \n /maincustomer \n /back",
                                      cancellationToken: ct);
        }

        private async Task SetName(ITelegramBotClient client, Update update, Order order, CancellationToken ct)
        {
           if (update.Message.Text == "/createorder")
           {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Введите название теста",
                                              cancellationToken: ct);
               return;
           }
                //Тут нужна функция показать кнопки
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    default:
                        order.Name = userText;
                        await Console.Out.WriteLineAsync($"Название заказа {order.Name}");
                        order.State = Order.OrdState.Named;
                        break;
                    }
                }
        }
        private async Task GetImg(ITelegramBotClient client, Update update, Order order, CancellationToken ct, string fileStoreName)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                              text: "Это просто кнопки для меню \n /maincustomer \n /back",
                                              cancellationToken: ct);
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
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
            string dirName = Path.Combine(@"C:\splittestmpbot\", update.Message.Chat.Id.ToString(), "customer", order.Id.ToString());
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

                // string desinationFilePath = Path.Combine(dirName, _filePath, fileStoreName);
                string desinationFilePath = Path.Combine(dirName, fileStoreName);
                await using FileStream fileStream = System.IO.File.OpenWrite(desinationFilePath);
                await client.DownloadFileAsync(filePath: _filePath, destination: fileStream);
            }
        }
        private async Task GetVoteOrder(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData, bool lowBal)
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

            if (lowBal) 
            {
                order.State = OrdState.BalanceLo;
                /*--------------------
                Сюда нужно запихать часть с текстом от BalanceLo и оставить ту, которая после ввода текста
                */
                return;
            }
            order.State = OrdState.BalanceOk;
            /*--------------------
            Сюда нужно запихать часть с текстом от BalanceOk и оставить ту, которая после ввода текста
            */
        }
        private async Task BalanceLo(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "На Вашем баллансе не хватает " + (userData.Balance - (order.VoteOrder-order.VoteActual)*userData.BalToVoteKoef).ToString() +
                                  " баллов для выполнения заказа.\n Предпочитаете пополнить балланс или изменить сумму заказа?",
                                  cancellationToken: ct);
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/changevoteorder":
                        userData.RequestToChangeVoteOrder = true;
                        order.State = Order.OrdState.LoadedB;
                        break;
                    case "/addtobalance":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "На какую сумму хотите пополнить балланс?",
                                                          cancellationToken: ct);
                        if ((update.Message.Text != null)&&(int.TryParse(update.Message.Text, out var result)))
                        {
                            userData.Balance += result;
                        }
                        order.State = Order.OrdState.LoadedB;
                        break;
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Введите сумму цифрами",
                                                          cancellationToken: ct);
                        break;
                }
            }
        }
        private async Task BalanceOk(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "На этом этапе можно запустить тест, скорректировать или отменить его",
                                  cancellationToken: ct);
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "Это просто кнопки для меню \n /changevoteorder \n /runtest \n /maincustomer",
                                  cancellationToken: ct);
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/changevoteorder":
                        userData.RequestToChangeVoteOrder = true;
                        order.State = Order.OrdState.LoadedB;
                        break;
                    case "/runtest":
                        order.State = Order.OrdState.Running;
                        break;
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выберите действие",
                                                          cancellationToken: ct);
                        break;
                }
            }
        }
        private async Task SendStatistic(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData)
        {
            await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: "Статистика по текущему заказу:" +
                                  $"- всего ответов: {order.VoteActual}" +
                                  $"- голосов за поз. А: {order.VoteA*100/order.VoteActual} %" +
                                  $"- голосов за поз. B: {order.VoteB * 100/order.VoteActual} %" +
                                  $"на тест потрачено {order.VoteActual/userData.BalToVoteKoef}",
                                  cancellationToken: ct);
        }
        private async Task Running(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData, bool lowBal)
        {
            if (lowBal)
            {
                order.State = OrdState.BalanceLo;
                return;
            }

            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/showstat":
                        await SendStatistic(client, update, order, ct, userData);
                        break;
                    case "/pause":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Постановка заказа на паузу, показы не производятся",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.Paused;
                        break;
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выберите действие",
                                                          cancellationToken: ct);
                        break;
                }
            }
        }
        private async Task Paused(ITelegramBotClient client, Update update, Order order, CancellationToken ct, User userData, bool lowBal)
        {
            if (lowBal)
            {
                order.State = OrdState.BalanceLo;
                return;
            }
            
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/showstat":
                        await SendStatistic(client, update, order, ct, userData);
                        break;
                    case "/pause":
                        order.State = Order.OrdState.Running;
                        break;
                    case "/maincustomer":
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Сброс заказа, возврат в меню заказчика",
                                                          cancellationToken: ct);
                        order.State = Order.OrdState.ToDelete;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выберите действие",
                                                          cancellationToken: ct);
                        break;
                }
            }
        }
        private async Task DeleteOrder(ITelegramBotClient client, Update update, long _orderIdx, Order _order, CancellationToken ct)
        {
            var userText = update.Message.Text;
            if (userText != null)
            {
                switch (userText)
                {
                    case "/delete":
                        OrderDict.Remove(_orderIdx);
                        //ShowCustomerMenu
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выбор создать заказ /createorder или посмотреть список заказов /showorderlist",
                                                          cancellationToken: ct);
                        break;
                    case "/pause":
                        _order.State = Order.OrdState.Paused;
                        break;
                    default:
                        await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                          text: "Выберите действие",
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
