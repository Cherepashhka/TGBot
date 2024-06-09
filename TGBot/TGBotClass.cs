using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bots.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using TGBot.ForConnection;
using Telegram.Bots.Http;
using Telegram.Bots;



namespace TGBot
{
    public class TGBotClass
    {
        TelegramBotClient BotClient = new TelegramBotClient("6571979384:AAE1HhXDSl_Mz46IaAi6Bn2wCkzkh2R-0TI");
        CancellationToken CancellationToken = new CancellationToken();
        ReceiverOptions ReceiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        //int Action = 0;
        public async Task BootUp()
        {
            BotClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, ReceiverOptions, CancellationToken);
            var BotMe = await BotClient.GetMeAsync();
            Console.WriteLine($"{BotMe.Username} стартував");
            Console.ReadKey();
        }

        private Task HandlerErrorAsync(ITelegramBotClient botclient, Exception exception, CancellationToken cancellationtoken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException _ApiRequestException => $"Помилка:\n{_ApiRequestException.ErrorCode}\n{_ApiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine($"Помилка: {ErrorMessage}");
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botclient, Update update, CancellationToken cancellationtoken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(BotClient, update.Message);
            }
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "Меню") message.Text = "/help";
            switch (message.Text.Split(' ')[0])
            {
                case "/start":
                    ReplyKeyboardMarkup replykeyboardmarkup = new(new[] { new KeyboardButton[] { "Меню" } }) { ResizeKeyboard = true };
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Привіт, це бот для пошуку скінів з CS2, підрахунку вартості інвентаря в Steam та списку улюблених предметів. Для перегляду функцій бота натисніть кнопку Меню або введіть команду /help.", replyMarkup: replykeyboardmarkup);
                    break;

                case "/help":
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "/skinprice <Name> - Знайти скін за назвою (замість пробілів в назві скіна ставити нижнє підкреслення!).\n"
                      + "/inventory <SteamID> - Підрахувати ціну інвентаря в Steam (по предметам CS2) за SteamID.\n"
                      + "/listpref - Вивести ваш список вподобань.\n"
                      + "/newpref <Name> - Занести скін в список вподобань за назвою (замість пробілів в назві скіна ставити нижнє підкреслення!).\n"
                      + "/deletepref <Name> - Видалити скін зі списку вподобань за назвою (замість пробілів в назві скіна ставити нижнє підкреслення!).\n"
                      + "/deleteallpref - Видалити всі скіни зі списку вподобань.\n"
                      + "/sticker - Стікер\n"
                      + "Приклад назви скіна для певних функцій: AK-47_Neon_Rider");
                    break;

                case "/skinprice":
                    if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
                        await GetWeaponByName(message);

                    else
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть назву скіна правильно.");

                    break;

                case "/inventory":
                    if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length == 17)
                        await GetInventoryPrice(message);

                    else
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть SteamID правильно.");

                    break;

                case "/newpref":
                    if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
                    {
                        await GetWeaponToPref(message);
                    }

                    else
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть назву скіна правильно.");

                    break;

                case "/listpref":
                    await ShowPreferenceList(message);
                    break;

                case "/deletepref":
                    if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
                    {
                        await DeleteFromPrefList(message);
                    }

                    else
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть назву скіна правильно.");

                    break;

                case "/deleteallpref":
                    await DeleteAllFromPrefList(message);
                    break;

                case "/sticker":
                    await BotClient.SendStickerAsync(message.Chat.Id, InputFile.FromUri("https://github.com/TelegramBots/book/raw/master/src/docs/sticker-dali.webp"));
                    break;

                default:
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Невідома команда. Будь ласка, використайте команду /help, щоб побачити список доступних команд.");
                    break;
            }
        }
        private async Task GetWeaponByName(Message message)
        {
            if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
            {
                ConnectionClient skinPriceClient = new ConnectionClient();
                var responce = skinPriceClient.GetSkinInfoAsync(message.Text.Split(' ')[1]);

                if (responce.Result != null)
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, $"Ім'я: {responce.Result.name}\nЦіна (в доларах): {responce.Result.price}$\nЗброя: {responce.Result.weapon}\nКартинка:");
                    await BotClient.SendPhotoAsync(message.Chat.Id, InputFile.FromUri(responce.Result.img));
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Повернутись на початок - /start");
                }
                else
                {
                    string errorMessage = $"Сталася помилка при спробі витягнути дані.";
                    Console.WriteLine(errorMessage);
                    await BotClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
                }
            }
            else
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, спробуйте ще раз і введіть нормальне ID.");
            }
        }

        private async Task GetWeaponToPref(Message message)
        {
            if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
            {
                ConnectionClient skinPriceClient = new ConnectionClient();
                await skinPriceClient.PostInPreferenceList(message.Text);
                await BotClient.SendTextMessageAsync(message.Chat.Id, "Успішно додано.\nПовернутись на початок - /start");
            }
        }
        private async Task ShowPreferenceList(Message message)
        {
            ConnectionClient skinPriceClient = new ConnectionClient();
            var responce = skinPriceClient.GetPreferenceList().Result;
            await BotClient.SendTextMessageAsync(message.Chat.Id, responce);
        }
        private async Task DeleteFromPrefList(Message message)
        {
            if (message.Text.Split(' ').Length == 2 && message.Text.Split(' ')[1].Length > 4)
            {
                ConnectionClient skinPriceClient = new ConnectionClient();
                await skinPriceClient.DeleteFromPreferenceList(message.Text.Split(' ')[1]);
                await BotClient.SendTextMessageAsync(message.Chat.Id, "Успішно видалено.\nПовернутись на початок - /start");
            }
        }
        private async Task DeleteAllFromPrefList(Message message)
        {
            ConnectionClient skinPriceClient = new ConnectionClient();
            await skinPriceClient.DeleteAllFromPreferenceList();
            await BotClient.SendTextMessageAsync(message.Chat.Id, "Успішно очищено список вподобань.\nПовернутись на початок - /start");
        }
        private async Task GetInventoryPrice(Message message)
        {
            ConnectionClient skinPriceClient = new ConnectionClient();
            await BotClient.SendTextMessageAsync(message.Chat.Id, await skinPriceClient.Inventory(message.Text.Split(' ')[1]));
        }
    }
}