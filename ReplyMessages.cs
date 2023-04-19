using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Telegram.Bots.Http;
using HtmlAgilityPack;
using static System.Net.WebRequestMethods;

namespace TelegBot
{
    
    internal partial class BotEngine
    {
        enum State { Menu, ScheduleMenu, ScheduleChooseGroup }
        static State currentState;

        public static void HandleUserMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            switch (message.Text.ToLower())
            {
                case "/start":
                case "повернутись в меню":
                    currentState = State.Menu;
                    MenuReply(botClient, message);
                    break;
                case "показати розклад":
                    if (currentState == State.Menu)
                    {
                        currentState = State.ScheduleMenu;
                        ScheduleOptionsReply(botClient, message);
                    }
                    else
                    {
                        ErrorReply(botClient, message);
                    }
                    break;
                case "обрати групу":
                    if(currentState == State.ScheduleMenu)
                    {
                        currentState = State.ScheduleChooseGroup;
                        EnterGroupNameReply(botClient, message);
                    }
                    break;
                default:
                    if(currentState == State.ScheduleChooseGroup)
                    {
                        currentState = State.ScheduleMenu;
                        ScheduleShow(botClient, message);
                    }
                    else
                    {
                        ErrorReply(botClient, message);
                    }
                    break;
            }
            return;
        }

        public static async Task MenuReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Вітаємо!");
            var buttons = new KeyboardButton[][]
            {
                    new KeyboardButton[]
                        {
                            new KeyboardButton("Показати розклад")
                        },

                        new KeyboardButton[]
                        {
                            new KeyboardButton("Показати свій розклад")
                        },

                        new KeyboardButton[]
                        {
                            new KeyboardButton("Налаштування")
                        }
            };
            var rkm = new ReplyKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(message.Chat.Id, "Оберіть дію", replyMarkup: rkm);
            return;
        }

        public static async Task ScheduleOptionsReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            var buttons = new KeyboardButton[][]
                {
                    new KeyboardButton[]
                        {
                            new KeyboardButton("Обрати групу")
                        },

                        new KeyboardButton[]
                        {
                            new KeyboardButton("Обрати викладача")
                        },

                        new KeyboardButton[]
                        {
                            new KeyboardButton("Повернутись в меню")
                        }
                };
            var rkm = new ReplyKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(message.Chat.Id, "Оберіть дію", replyMarkup: rkm);
            return;
        }

        public static async Task ScheduleShow(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            string url = "http://213.108.47.12:3050/cgi-bin/timetable.cgi?n=700&group=";
            url += message.Text;
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            HtmlWeb webGet = new HtmlWeb();
            webGet.OverrideEncoding = Encoding.GetEncoding(1251);
            HtmlDocument doc = webGet.Load(url);
            string res = doc.DocumentNode.InnerText;
            //await botClient.SendTextMessageAsync(message.Chat, "Я в функції ScheduleShow, яка робить гет реквест і виводить елементи хтмл сторінки, але чогось не робе");
            //HtmlNode myNode = doc.DocumentNode.SelectSingleNode("div//[@class='row']");
            //await botClient.SendTextMessageAsync(message.Chat, myNode.InnerText);
            await botClient.SendTextMessageAsync(message.Chat, res);
        }

        public static async Task EnterGroupNameReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Enter group CODE!!! TEMPORARY");
        }

        public static async Task ErrorReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Something went wrong :)");
        }
    }
}
