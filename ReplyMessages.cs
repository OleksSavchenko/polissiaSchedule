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
using Telegram.Bots.Types;
using System.Runtime.Intrinsics.Arm;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TelegBot
{


    internal partial class BotEngine
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public enum State { Menu, ScheduleMenu, ScheduleChooseGroup, ScheduleChooseTeacher }

        
        static Dictionary<long, State> usersStatus = new Dictionary<long, State>();

        

        public static async void HandleUserMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {

            try
            {
                switch (message.Text.ToLower())
                {
                    case "/start":
                    case "повернутись в меню":
                        await MenuReply(botClient, message);
                        break;
                    case "показати розклад":
                        if (!usersStatus.ContainsKey(message.From.Id))
                        {
                            usersStatus.Add(message.From.Id, State.ScheduleMenu);
                            await ScheduleOptionsReply(botClient, message);
                        }
                        else throw new Exception("An error occurred");
                        break;
                    case "показати свій розклад":
                        string[] userDataArr = GetUser(message.From.Id);
                        if (userDataArr[0] != null)
                        {
                            message.Text = userDataArr[1];
                            if (userDataArr[0] == "student")
                            {
                                usersStatus.Remove(message.From.Id);
                                await GroupScheduleShow(botClient, message);
                            }
                            else if (userDataArr[0] == "teacher")
                            {
                                usersStatus.Remove(message.From.Id);
                                await TeacherScheduleShow(botClient, message);
                            }
                            else throw new Exception("У вас ще немає власного розкладу!");
                        }
                        break;
                    case "обрати групу":
                        if (usersStatus[message.From.Id] == State.ScheduleMenu)
                        {
                            usersStatus[message.From.Id] = State.ScheduleChooseGroup;
                            await EnterGroupNameReply(botClient, message);
                        }
                        else throw new Exception("An error occurred");
                        break;
                    case "обрати викладача":
                        if (usersStatus[message.From.Id] == State.ScheduleMenu)
                        {
                            usersStatus[message.From.Id] = State.ScheduleChooseTeacher;
                            await EnterTeacherNameReply(botClient, message);
                        }
                        else throw new Exception("An error occurred");
                        break;
                    default:

                        if (ScheduleOptionVerification(message.Text, usersStatus[message.From.Id]))
                        {
                            if (usersStatus[message.From.Id] == State.ScheduleChooseGroup)
                            {
                                if (GetUser(message.From.Id)[0] == null)
                                {
                                    DBUserInsert(message.From.Id, "student", message.Text);
                                }
                                usersStatus.Remove(message.From.Id);
                                await GroupScheduleShow(botClient, message);
                                await MenuReply(botClient, message);
                            }
                            else if (usersStatus[message.From.Id] == State.ScheduleChooseTeacher)
                            {
                                if (GetUser(message.From.Id)[0] == null)
                                {
                                    DBUserInsert(message.From.Id, "teacher", message.Text);
                                }
                                usersStatus.Remove(message.From.Id);
                                await TeacherScheduleShow(botClient, message);
                                await MenuReply(botClient, message);
                            }
                            else
                            {
                                throw new Exception("An error occurred");
                            }
                        }
                        else throw new Exception("Не коректна назва групи/ім'я викладача");
                        break;
                }
            }
            catch (Exception ex)
            {
                await ErrorReply(botClient, message, "An error occurred!");
                log.Error(ex.Message);
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
                List<string> days = GetDaysList(res);
                for (int i = 0; i < days.Count; i++)
                {
                    await botClient.SendTextMessageAsync(message.Chat, StringFormatter(days[i]));
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
            List<string> days = GetDaysList(res);
            for (int i = 0; i < days.Count; i++)
            {
                await botClient.SendTextMessageAsync(message.Chat, StringFormatter(days[i]));
            }
        }

        public static async Task EnterGroupNameReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Введіть назву групи");
        }
        public static async Task EnterTeacherNameReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat, "Введіть ім'я викладача");
        }

        public static async Task ErrorReply(ITelegramBotClient botClient, Telegram.Bot.Types.Message message, string errorMessage)
        {

            await botClient.SendTextMessageAsync(message.Chat, errorMessage);
            usersStatus.Remove(message.From.Id);
            await MenuReply(botClient, message);
        }

        public static string GetGroupID(string message)
        {

            string query = $"select * from groups where groupName = '{message.ToLower()}'";
            MySqlCommand cmd = new MySqlCommand(query, Program.conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.ReadAsync();
            string groupID = "";
            try
            {
                groupID = reader["groupID"].ToString();
            }
            catch (Exception)
            {
                //reader.CloseAsync();
            }
            reader.CloseAsync();
            return groupID;
        }
        public static string GetTeacherID(string message)
        {
            string query = $"select * from teachers where teacherName = '{message.ToLower()}'";
            MySqlCommand cmd = new MySqlCommand(query, Program.conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.ReadAsync();
            string teacherID = "";
            try
            {
                teacherID = reader["teacherID"].ToString();
            }
            catch (Exception)
            {

            }
            reader.CloseAsync();
            return teacherID;
        }

        public static string StringFormatter(string daySchedule)
        {
            //довжина рядка в якому визначений номер та час пари
            const int lengthOfLectureTimeSubstring = 11;
            //спліт по парах
            Regex dayRegex = new Regex(@"(?<=[0-9:]{11})");
            List<string> dayLectures = new List<string>();
            string[] subj = dayRegex.Split(daySchedule);
            for (int i = 0; i < subj.Length - 1; i++)
            {
                if (i == 0) dayLectures.Add(subj[i].Remove(subj[i].Length - lengthOfLectureTimeSubstring));
                string date = subj[i].Substring(subj[i].Length - lengthOfLectureTimeSubstring);
                date += subj[i + 1];
                string z;
                if (i != subj.Length - 2) z = date.Remove(date.Length - lengthOfLectureTimeSubstring);
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
                if (lecture.Substring(11).Trim() == "")
                {
                    continue;
                }
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
                    string firstS = lecture.Substring(11, index - lengthOfLectureTimeSubstring);
                    string secondS = lecture.Substring(index);
                    result += $"{firstS}\n{secondS}";

                }
                else
                {
                    result += $"{lecture.Substring(lengthOfLectureTimeSubstring)}";
                }
                result += "\n\n";
            }


            return result;
        }
        public static List<string> GetDaysList(string htmlText)
        {
            htmlText = Regex.Replace(htmlText, @"\s+", " ");

            //спліт по дням
            Regex daysRegex = new Regex(@"(?<=[0-9]{2}\.[0-9]{2}\.[0-9]{4})");
            string[] lines = daysRegex.Split(htmlText);
            List<string> days = new List<string>();
            for (int i = 4; i < lines.Length - 1; i++)
            {
                string date = lines[i].Substring(lines[i].Length - 10);
                date += lines[i + 1];
                string z = date.Remove(date.Length - 10);
                days.Add(z);
            }
            return days;
        }
        public static string[] GetUser(long id)
        {
            string query = $"select * from users where userID = '{id.ToString().ToLower()}'";
            MySqlCommand cmd = new MySqlCommand(query, Program.conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.ReadAsync();
            string[] userArr = new string[2];
            try
            {
                userArr[0] = reader["userStatus"].ToString();
                userArr[1] = reader["userSchedule"].ToString();
            }
            catch (Exception)
            {
                reader.CloseAsync();
                return userArr;
            }
            reader.CloseAsync();
            return userArr;
        }
        public static void DBUserInsert(long userID, string status, string schedule)
        {
            string query = $"INSERT INTO `users`(`userID`, `userSchedule`, `userStatus`) VALUES('{userID}', '{schedule}', '{status}')";
            MySqlCommand cmd = new MySqlCommand(query, Program.conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.ReadAsync();
            reader.CloseAsync();
        }
        public static bool ScheduleOptionVerification(string scheduleOption, State status)
        {
            if (status == State.ScheduleChooseGroup && GetGroupID(scheduleOption) != "")
            {
                return true;
            }
            else if (status == State.ScheduleChooseTeacher && GetTeacherID(scheduleOption) != "")
            {
                return true;
            }
            else return false;
        }

    }
}
