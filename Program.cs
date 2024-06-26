﻿using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OtusBasicGradWork
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Получаем значение токена из переменной среды
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Machine)!;
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("Telegram bot token is not set in the environment variables.");
                return;
            }

            var botClient = new TelegramBotClient(botToken);
            using CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            var _executor = new Executor();
            var _customer = new Customer();
            var _customerTestBalance = 1000;
            var dictStatesOfUsers = new Dictionary<long, User>();

            botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                                     pollingErrorHandler: HandleErrorAsync,
                                     receiverOptions: new ReceiverOptions()
                                     {
                                         AllowedUpdates = Array.Empty<UpdateType>()
                                     },
                                     cancellationToken: token);

            var me = await botClient.GetMeAsync();

            var apiTestResult = await botClient.TestApiAsync();

            if (apiTestResult)
            {
                Console.WriteLine("API Telegram доступно. Соединение установлено.");
            }
            else
            {
                Console.WriteLine("Ошибка при проверке API Telegram или соединении с серверами.");
                return;
            }

            Console.WriteLine($"Бот запущен для @{me.Username}. Нажмите любую клавишу для остановки...");
            Console.ReadKey();

            cts.Cancel();

            Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cts)
            {
                Console.WriteLine("Свалились в ErrorHandler");
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
            async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
            {
                //if (update.Message == null) return;
                if (update.Message is not { } message)
                    return;

                if (!dictStatesOfUsers.TryGetValue(update.Message.Chat.Id, out var userData))
                {
                    dictStatesOfUsers.Add(update.Message.Chat.Id, new User(_customerTestBalance, ChatMode.Initial));
                    userData = dictStatesOfUsers[update.Message.Chat.Id];
                }

                if ((update.Message.Text == "/main") || (update.Message.Text == "/start"))
                {
                    await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                  text: $"Привет, {update.Message.Chat.FirstName.ToString()}. Мы в самом начале меню \n"/* +
                                  "/customer - заказчик\n" +
                                  "/tester - тестировщик"*/,
                                  cancellationToken: ct);
                    userData.ChatMode = ChatMode.Initial;
                    await SendMenu(client, update, ct);
                }
                else
                {
                    switch (userData.ChatMode)
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
            }
            static async Task SendMenu(ITelegramBotClient client, Update update, CancellationToken ct)
            {
                await client.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                  text: "Выберите роль\n" +
                                                  "/customer - заказчик\n" +
                                                  "/tester - тестировщик",
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
}
