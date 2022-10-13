using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SignalRChat.Models;

namespace SignalRChat.HubClass
{

    /// <summary>
    /// SignalR Hub
    /// </summary>
    [HubName("groupChatHub")]
    public class GroupChatHub : Hub
    {
       /// <summary>
       /// return username from cookie.
       /// </summary>
        private string UserName
        {
            get
            {
                try
                {
                    return Context.RequestCookies["UserInfo"] != null ?
                    HttpUtility.UrlDecode(Context.RequestCookies["UserInfo"].Value) : "";
                }
                catch (Exception)
                {
                    return Constants.lastUserLoggedIn;
                    //return dbHelper.QueryFirstOrDefault<string>("SELECT TOP 1 UserName from UserLogs order by LoggedDate desc");
                }
                
            }
        }

        /// <summary>
        /// list of online users
        /// </summary>
        private static Dictionary<string, int> _onlineUsers = new Dictionary<string, int>();

        /// <summary>
        /// hub OnConnected 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            Connected();
            return base.OnConnected();
        }


        /// <summary>
        /// Hub OnReconnected
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            Connected();
            return base.OnReconnected();
        }



        private void Connected()
        {
            // 
            if (!_onlineUsers.ContainsKey(UserName))
            {
                _onlineUsers.Add(UserName, 1);

                PopulateUserList();
                Clients.Group("GROUP-Admin").publishSystemMsg(FormatMsg("Everyone", UserName + " Joined chat"));
            }
            else
            {
                _onlineUsers[UserName] = _onlineUsers[UserName] + 1;
            }

            Groups.Add(Context.ConnectionId, "GROUP-" + UserName);
            if(UserName != "Admin") { 
                LoadChatHistory(UserName);
            }
        }



        /// <summary>
        /// Hub OnDisconnected
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            _onlineUsers[UserName] = _onlineUsers[UserName] - 1;

            if (_onlineUsers[UserName] == 0)
            {
                _onlineUsers.Remove(UserName);

                PopulateUserList();
                Clients.Group("GROUP-Admin").publishSystemMsg(FormatMsg("Everyone", UserName + " left Chat"));
            }

            Groups.Remove(Context.ConnectionId, "GROUP-" + UserName);
            if (UserName != "Admin")
            {
                EmailChat();
            }
            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// Send Message to All or Admin
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public void SendMsg(string toUser, string msg)
        {
            if (toUser == "Everyone")
            {
                Clients.Others.publishMsg(FormatMsg(UserName, msg));
            }
            else
            {
                UserChat.SaveChatMessage(UserName, toUser, msg);
                Clients.Groups(new List<string> { "GROUP-" + toUser }).publishMsg(FormatMsg(UserName, msg, toUser));
            }
        }
        /// <summary>
        /// Will update Online Users list for admin
        /// </summary>
        public void PopulateUserList()
        {
            Clients.Group("GROUP-Admin").publishUsers(_onlineUsers.Where(i => i.Key.ToString() != "Admin"));
            
        }
        public void ReloadChatHistory()
        {
            foreach (var item in _onlineUsers.Where(i => i.Key.ToString() != "Admin"))
            {
                LoadChatHistory(item.Key);
            }
        }
        public void LoadChatHistory(string name)
        {
            var id = User.GetUserIdByName(name);
            if (id>0)
            {
                var history = UserChat.GetByUserId(id);
                if (history.Count>0)
                {
                    Clients.Group("GROUP-Admin").publishChatHistory(history, name, false);
                }
            }
        }

        public void EmailChat()
        {
            var id = User.GetUserIdByName(UserName);
            if (id > 0)
            {
                UserChat.EmailChatHistory(id);
            }                        
        }
        /// <summary>
        /// Format message 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        private dynamic FormatMsg(string name, string msg, string toUser="")
        {
            return new { Name = name, Msg = HttpUtility.HtmlEncode(msg), Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ToUser = toUser };
        }
    }


    /// <summary>
    /// GroupCHat class to be used by controller.
    /// </summary>
    public class GroupChat
    {
        /// <summary>
        /// Clients
        /// </summary>
        private IHubConnectionContext<dynamic> Clients { get; set; }

        private readonly static GroupChat _instance = new GroupChat(GlobalHost.ConnectionManager.GetHubContext<GroupChatHub>().Clients);

        private GroupChat(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        public static GroupChat Instance
        {
            get { return _instance; }
        }


        /// <summary>
        /// SendSystemMsg that controller can call using chat instance.
        /// </summary>
        /// <param name="msg"></param>
        public void SendSystemMsg(string msg)
        {
            Clients.Group("GROUP-Admin").publishSystemMsg( FormatMsg("Everyone",msg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }
        public void SendMsg(string toUser, string msg, string UserName)
        {
            UserChat.SaveChatMessage(UserName, toUser, msg);
            Clients.Groups(new List<string> { "GROUP-" + toUser }).publishMsg(FormatMsg(UserName, msg, toUser));
        }

        private dynamic FormatMsg(string name, string msg, string toUser = "")
        {
            return new { Name = name, Msg = HttpUtility.HtmlEncode(msg), Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ToUser = toUser };
        }
    }
}