using SendGrid;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using NLog;

namespace WebGallery.Utilities
{
    public class SendGridEmailHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void SendAsync(string subject, string body, string from, string fromName, string to)
        {
            SendAsync(subject, body, from, fromName, to, null);
        }

        public static void SendAsync(string subject, string body, string from, string fromName, string to, string cc)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApplicationException("Unable to get an SendGrid API Key.");
            }

            var transportWeb = new Web(apiKey);
            transportWeb.DeliverAsync(CreateMessage(subject, body, from, fromName, to, cc));
        }

        public static SendGridMessage CreateMessage(string subject, string body, string from, string fromName, string to, string cc)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException($"Invalid argument: {nameof(to)}");
            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException($"Invalid argument: {nameof(from)}");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException($"Invalid argument: {nameof(subject)}");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException($"Invalid argument: {nameof(body)}");

            var message = new SendGridMessage();
            message.Subject = subject;
            message.Html = body;
            message.From = new MailAddress(from, fromName);
            message.AddTo(to.Trim().Trim(',').Replace(';', ',').Split(',').Distinct().Where(e => !string.IsNullOrWhiteSpace(e)));
            if (!string.IsNullOrWhiteSpace(cc)) message.AddCc(cc);
            logger.Log(LogLevel.Info, string.Format("SendGrid Email: {0}",
                string.Format("Email From: {0}<{1}> To:{2} Subject:{3}", message.From.DisplayName, message.From.Address,
                string.Join(",", message.To.AsEnumerable()), message.Subject)));
            return message;
        }
    }
}