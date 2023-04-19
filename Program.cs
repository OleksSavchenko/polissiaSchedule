using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
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


        static void Main(string[] args)
        {
            Console.WriteLine("Hehe: " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(BotEngine.HandleUpdateAsync, BotEngine.HandleErrorAsync, receiverOptions, cancellationToken);

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
