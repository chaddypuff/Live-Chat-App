using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SignalRChat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordKey { get; set; }
        public bool Admin { get; set; }
        public string Note { get; set; }
        public DateTime LastLogin { get; set; }
        public string ConnectionId { get; set; }

        internal static int NewUser(User u)
        {
            u.PasswordKey = Guid.NewGuid().ToString().Replace("-", "");
            u.Password = AesOperation.EncryptString(u.PasswordKey, u.Password);
            return Convert.ToInt32(dbHelper.Users.Create(u));
        }
        internal static User Authenticate(string userEmail, string password)
        {
            var user = dbHelper.Users.GetAll().FirstOrDefault(x => x.Email == userEmail);//.GetByUserName(userName.Trim());
            if (user == null)
            {
                return null;
            }
            return password.Equals(AesOperation.DecryptString(user.PasswordKey, user.Password)) ? user : null;
        }
        internal static int SaveAnonymousUser(string userName, string email, string password="", string note="", string connctionId="")
        {
            var user = dbHelper.Users.GetAll().FirstOrDefault(x => x.Name == userName);
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                var newUser = new User()
                {
                    Name = userName,
                    Email = email,
                    Password = password,
                    PasswordKey = "",
                    Admin = false,
                    ConnectionId = connctionId,
                    Note = note,
                    LastLogin = DateTime.Now
                };
                //send account creation email
                return NewUser(newUser);
            }

        }

        internal static string GetEmailById(int id)
        {
            return dbHelper.Users.GetAll().Where(x => x.Id == id).Select(x => x.Email).FirstOrDefault();
        }

        internal static int GetUserIdByName(string userName)
        {
            return dbHelper.Users.GetAll().Where(x => x.Name == userName).Select(x => x.Id).FirstOrDefault();                    
        }
    }
}