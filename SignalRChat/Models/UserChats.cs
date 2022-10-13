using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRChat.Models
{
    public class UserChat
    {
        public int Id { get; set; }
        public int ToUserId { get; set; }
        public int FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public string Message { get; set; }
        public DateTime SentDateTime { get; set; }

        public static List<UserChat> GetByUserId(int userId)
        {
            return dbHelper.UserChats.GetAll().Where(x => x.ToUserId == userId || x.FromUserId == userId).OrderBy(x=>x.SentDateTime).ToList();
        }
        internal static void SaveChatMessage(string from, string to, string msg)
        {
            var chat = new UserChat()
            {
                ToUserId = User.GetUserIdByName(to),
                FromUserId = User.GetUserIdByName(from),
                ToUserName = to,
                FromUserName = from,
                Message = msg,
                SentDateTime = DateTime.Now
            };
            dbHelper.UserChats.Create(chat);
        }

        internal static void EmailChatHistory(int id)
        {
            var history = GetByUserId(id);
            if (history.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var chat in history)
                {
                    sb.Append($"<p>{chat.FromUserName}: {chat.Message} ({chat.SentDateTime.ToString("dd MM yyyy HH:mm:ss")})<p>");
                }
                var email = User.GetEmailById(id);

                new EmailHelper().SendChatHistory(email, sb.ToString());
            }
        }
    }
}