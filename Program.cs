using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots.Types;

namespace TelegBot
{
    internal class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("6275603213:AAGcU2TVg_wTnRyvXLXTfjxMnTysGhb-e3c");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start" || message.Text.ToLower() == "повернутись в меню")
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
                else if (message.Text.ToLower() == "показати розклад")
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
                await botClient.SendTextMessageAsync(message.Chat, "Something went wrong :)");
            }


        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hehe: " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            
            Console.ReadLine();
        }

        static string GetRequest(string url)
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            HtmlWeb webGet = new HtmlWeb();
            webGet.OverrideEncoding = Encoding.GetEncoding(1251);
            HtmlDocument doc = webGet.Load(url);
            string res = doc.DocumentNode.InnerText;
            return res;
            //Console.WriteLine(doc.DocumentNode.OuterHtml);
            //Console.ReadLine();

            //Console.WriteLine(res);
            //HtmlNode myNode = doc.DocumentNode.SelectSingleNode("div//[@class='row']");
            //Console.WriteLine(myNode.InnerText);
            //if (myNode != null) return doc.DocumentNode.OuterHtml;
            //else return "Nothing found";
        }
    }

}
