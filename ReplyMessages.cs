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
using MySqlConnector;
using Telegram.Bot.Types;
using System.Text.RegularExpressions;

namespace TelegBot
{

    
    internal partial class BotEngine
    {
        const string server = "localhost";
        const string databaseName = "telegrambotdb";
        const string username = "root";
        const string password = "usbw";
        const string connString = $"SERVER={server};DATABASE={databaseName};UID={username};PASSWORD={password};";

        enum State { Menu, ScheduleMenu, ScheduleChooseGroup, ScheduleChooseTeacher }
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
                    if (currentState == State.ScheduleMenu)
                    {
                        currentState = State.ScheduleChooseGroup;
                        EnterGroupNameReply(botClient, message);
                    }
                    else ErrorReply(botClient, message);  
                    break;
                case "обрати викладача":
                    if (currentState == State.ScheduleMenu)
                    {
                        currentState = State.ScheduleChooseTeacher;
                        EnterTeacherNameReply(botClient, message);
                    }
                    break;
                default:
                    if(currentState == State.ScheduleChooseGroup)
                    {  
                        currentState = State.Menu;
                        GroupScheduleShow(botClient, message);
                        MenuReply(botClient, message);
                    }
                    else if (currentState == State.ScheduleChooseTeacher)
                    {
                        currentState = State.Menu;
                        TeacherScheduleShow(botClient, message);
                        MenuReply(botClient, message);
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
            //await botClient.SendTextMessageAsync(message.Chat, "Вітаємо!");
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

        public static async Task GroupScheduleShow(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            string url = "http://rozklad.znau.edu.ua/cgi-bin/timetable.cgi?n=999&group=";

            url += GetGroupID(message.Text);
            
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            HtmlWeb webGet = new HtmlWeb();
            webGet.OverrideEncoding = Encoding.GetEncoding(1251);
            HtmlDocument doc = webGet.Load(url);
            string res = doc.DocumentNode.InnerText;
            for(int i = 0; i < 5; i++)
            {
                await botClient.SendTextMessageAsync(message.Chat, StringFormatter(res, i));
            }
        }
        public static async Task TeacherScheduleShow(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            string url = "http://rozklad.znau.edu.ua/cgi-bin/timetable.cgi?n=999&teacher=";


            url += GetTeacherID(message.Text);

            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            HtmlWeb webGet = new HtmlWeb();
            webGet.OverrideEncoding = Encoding.GetEncoding(1251);
            HtmlDocument doc = webGet.Load(url);
            string res = doc.DocumentNode.InnerText;
            await botClient.SendTextMessageAsync(message.Chat, res);
        }

        public static async Task EnterGroupNameReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Введіть назву групи");
        }
        public static async Task EnterTeacherNameReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Введіть ім'я викладача");
        }

        public static async Task ErrorReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Something went wrong :)");
        }

        public static string GetGroupID(string message)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();
            string query = $"select * from groups where groupName = '{message.ToLower()}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            return reader["groupID"].ToString();
        }
        public static string GetTeacherID(string message)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();
            string query = $"select * from teachers where teacherName = '{message.ToLower()}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            return reader["teacherID"].ToString();
        }

        public static string StringFormatter(string htmlText, int dayNumber)
        {
            htmlText = Regex.Replace(htmlText, @"\s+", " ");

            //спліт по дням
            Regex daysRegex = new Regex(@"(?<=[0-9]{2}.[0-9]{2}.[0-9]{4})");
            string[] lines = daysRegex.Split(htmlText);
            List<string> days = new List<string>();
            for (int i = 4; i < lines.Length - 1; i++)
            {
                string date = lines[i].Substring(lines[i].Length - 10);
                date += lines[i + 1];
                string z = date.Remove(date.Length - 10);
                days.Add(z);
            }

            //спліт по парах
            Regex dayRegex = new Regex(@"(?<=[0-9:]{11})");
            List<string> dayLectures = new List<string>();
            string day = days[dayNumber];
            string[] subj = dayRegex.Split(day);
            for (int i = 0; i < subj.Length - 1; i++)
            {
                if (i == 0) dayLectures.Add(subj[i].Remove(subj[i].Length - 11));
                string date = subj[i].Substring(subj[i].Length - 11);
                date += subj[i + 1];
                string z;
                if (i != subj.Length - 2) z = date.Remove(date.Length - 11);
                else z = date;
                dayLectures.Add(z);
            }
            if (dayLectures[dayLectures.Count - 1].Contains("ПП По"))
            {
                dayLectures[dayLectures.Count - 1] = dayLectures[dayLectures.Count - 1].Substring(0, dayLectures[dayLectures.Count - 1].IndexOf("ПП По") - 2);
            }

            //обробка вигляду пар
            string result = "";
            string dayDate = $"{dayLectures[0].Substring(0)}\n\n";
            result += dayDate;
            dayLectures.RemoveAt(0);
            foreach (string lecture in dayLectures)
            {
                string time = $"{lecture.Substring(1, 5)} - {lecture.Substring(6, 5)} ";
                result += time;
                if (lecture.IndexOf("(підгрупа 1)") != -1 && lecture.IndexOf("(підгрупа 2)") != -1)
                {
                    int index = lecture.IndexOf("(підгрупа 1)");
                    string prekol = lecture.Substring(index);

                    for (int i = 0; i < 4; i++)
                    {
                        index = prekol.IndexOf(" ") + 1;
                        prekol = prekol.Substring(index);
                    }
                    index = lecture.IndexOf(prekol);
                    string firstS = lecture.Substring(11, index - 11);
                    string secondS = lecture.Substring(index);
                    result += $"{firstS}\n{secondS}";

                }
                else
                {
                    result += $"{lecture.Substring(11)}";
                }
                result += "\n\n";
            }


            return result;
        }
    }
}
