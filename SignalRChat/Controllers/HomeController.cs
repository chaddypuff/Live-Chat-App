using SignalRChat.HubClass;
using SignalRChat.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Xml.Linq;

namespace SignalRChat.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            var cookieUserName = Request.Cookies["UserInfo"];
            if (cookieUserName == null) 
            {
                return RedirectToAction("User");
            }
            ViewBag.UserName = HttpUtility.UrlDecode(cookieUserName.Value);
            if (cookieUserName.Value.Equals("admin") || cookieUserName.Value.Equals("Admin"))
            {
                return Redirect("Home/AdminLogin");
            }
            return View();
        }
        public ActionResult Admin()
        {
            var cookieUserName = Request.Cookies["UserInfo"];
            if (cookieUserName == null)
            {
                return RedirectToAction("NewUser");
            }
            ViewBag.UserName = HttpUtility.UrlDecode(cookieUserName.Value);
            return View();
        }

        public ActionResult NewUser()
        {
            return RedirectToAction("User");// View();
        }

        
        [HttpPost]
        public ActionResult NewUser(string userName, string userEmail, string userNote)
        {
            SignalRChat.Models.User.SaveAnonymousUser(userName, userEmail, userNote);
            Session["userName"] = userName;
            Response.Cookies.Add(new HttpCookie("UserInfo", HttpUtility.UrlEncode(userName)));
            dbHelper.Execute("INSERT INTO UserLogs VALUES (@name)", new { name = userName });
           

            return RedirectToAction("Index");// Redirect(Constants.GetSiteURL() + ("/"));
         
        }
        public ActionResult User()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UserLogin(string userName, string userEmail, string userNote, string userPassword)
        {
            SignalRChat.Models.User.SaveAnonymousUser(userName, userEmail, userPassword, userNote);
            Session["userName"] = userName;
            Response.Cookies.Add(new HttpCookie("UserInfo", HttpUtility.UrlEncode(userName)));
            Constants.lastUserLoggedIn = userName;
           
            return Json(new { msg = "success"});// Redirect(Constants.GetSiteURL() + ("/"));
        }
        public ActionResult AdminLogin()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AdminLogin(string userEmail,string userPassword)
        {
                var user = SignalRChat.Models.User.Authenticate(userEmail, userPassword);
                if (user != null)
                {
                    Response.Cookies.Add(new HttpCookie("UserInfo", HttpUtility.UrlEncode(user.Name)));
                    return RedirectToAction("Admin");
                }
                else
                {
                    ViewBag.Msg = "Login Failed";
                    return View();
                }
        }
        public ActionResult SignOut()
        {
            if (Request.Cookies["UserInfo"] != null)
            {
                //Fetch the Cookie using its Key.
                HttpCookie nameCookie = Request.Cookies["UserInfo"];

                //Set the Expiry date to past date.
                nameCookie.Expires = DateTime.Now.AddDays(-1);

                //Update the Cookie in Browser.
                Response.Cookies.Add(nameCookie);
            }
            return RedirectToAction("/");
        }
        
        public ActionResult CreateAdminAccount()
        {
            var email = ConfigurationManager.AppSettings["Admin-DefaultEmail"];
            if (dbHelper.Users.GetAll().FirstOrDefault(x => x.Email == email) == null)
            {
                var newUser = new SignalRChat.Models.User()
                {
                    Name = ConfigurationManager.AppSettings["Admin-DefaultName"],
                    Email = email,
                    Password = ConfigurationManager.AppSettings["Admin-DefaultPassword"],
                    PasswordKey = "",
                    Admin = true,
                    ConnectionId = "",
                    Note = "",
                    LastLogin = DateTime.Now
                };
                SignalRChat.Models.User.NewUser(newUser);
            }
            
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult LoadChat(string name)
        {
            List<UserChat> history = new List<UserChat>();
            var id = SignalRChat.Models.User.GetUserIdByName(name);
            if (id > 0)
            {
                history = UserChat.GetByUserId(id);
            }
            return Json(new { data = history });
        }
        //for future use if we want to send message through controller

        //public ActionResult SendSystemMsg()
        //{
        //    return View();
        //}

        [HttpPost]
        public string SendMsg(string toUser, string msg, string userName)
        {
            GroupChat.Instance.SendMsg(toUser, msg, userName);
            return "s";
        }

    }
}