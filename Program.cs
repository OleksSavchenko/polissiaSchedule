using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegBot
{
    internal class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, GetRequest("http://213.108.47.12:3050/cgi-bin/timetable.cgi?n=700&group=2491"));
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Ещкере");
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
            //Console.WriteLine(doc.DocumentNode.OuterHtml);
            //Console.ReadLine();
            string res = doc.DocumentNode.InnerText;
            return res;
            //Console.WriteLine(res);
            //HtmlNode myNode = doc.DocumentNode.SelectSingleNode("div//[@class='row']");
            //Console.WriteLine(myNode.InnerText);
            //if (myNode != null) return doc.DocumentNode.OuterHtml;
            //else return "Nothing found";
        }
    }

}
