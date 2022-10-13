using SignalRChat.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;


namespace SignalRChat
{
    public class EmailHelper
    {
        private readonly string smtpHost = ConfigurationManager.AppSettings["SMTP-Host"];
        private readonly int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTP-Port"]);
        private readonly bool smtpEnableSSL = ConfigurationManager.AppSettings["SMTP-UseSSL"]=="true";
        private readonly bool smtpUseAuthentication = ConfigurationManager.AppSettings["SMTP-UseAuthentication"]=="true";
        private readonly string smtpEmail = ConfigurationManager.AppSettings["SMTP-Email"];
        private readonly string smtpPassword = ConfigurationManager.AppSettings["SMTP-Password"];
        private readonly string smtpSendFrom = ConfigurationManager.AppSettings["SMTP-SendFrom"];
        private readonly string smtpDisplayName = ConfigurationManager.AppSettings["SMTP-DisplayName"];

        internal string SendEmail(string SendToEmail, string Subject, string EmailBody)
        {
            //Email Message content
            MailMessage mail = new MailMessage();
            mail.To.Add(SendToEmail);
            mail.From = new MailAddress(smtpSendFrom, smtpDisplayName, System.Text.Encoding.UTF8);
            mail.Subject = Subject;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = EmailBody;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            //Sender email configuration
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(smtpEmail, smtpPassword);
            client.Port = smtpPort;
            client.Host = smtpHost;
            client.EnableSsl = smtpEnableSSL;
            try
            {
                client.Send(mail);
                return "success";
            }
            catch (Exception ex) { 
                return ex.ToString(); 
            }
        }

        internal void SendChatHistory(string email, string emailContent)
        {
            SendEmail(email, "Chat History", emailContent);
        }

        internal bool SendContactEmail(string sendToEmail, string contactname, string contactemail, string contactSubject, string contactmessage)
        {
            try
            {
                string message = $"<h3>{contactname}</h3><h5>{contactSubject}</h5><h4>{contactemail}</h4><h4>{contactmessage}</h4>";
                return SendEmail(sendToEmail, contactSubject + " | " + contactemail, message) == "success";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}