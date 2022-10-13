using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace SignalRChat
{
    public class Constants
    {
        public static string lastUserLoggedIn = "";
        public static string ConvertToJson(object data)
        {
            var serializer = new JavaScriptSerializer();
            var JsonData = serializer.Serialize(data);
            return JsonData;
        }
        public static bool IsUserAuthenticated()
        {
            return HttpContext.Current.GetOwinContext().Authentication.User.Identity.IsAuthenticated;
        }
        public static string GetSiteURL()
        {
            return System.Configuration.ConfigurationManager.AppSettings["Site-URL"];
        }
      
        private static Random rnd = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}