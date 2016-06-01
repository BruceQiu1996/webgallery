using SendGrid;
using System;
using System.Configuration;
using System.Net.Mail;

namespace WebGallery.Utilities
{
    public class SendGridEmailHelper
    {
        public static void SendAsync(string to, string from, string fromName, string subject, string body)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApplicationException("Unable to get an SendGrid API Key.");
            }

            var transportWeb = new Web(apiKey);
            transportWeb.DeliverAsync(CreateMessage(to, from, fromName, subject, body));
        }

        public static SendGridMessage CreateMessage(string to, string from, string fromName, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException($"Invalid argument: {nameof(to)}");
            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException($"Invalid argument: {nameof(from)}");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException($"Invalid argument: {nameof(subject)}");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException($"Invalid argument: {nameof(body)}");

            var message = new SendGridMessage();
            message.AddTo(to.Trim().Trim(',').Replace(';', ',').Split(','));
            message.From = new MailAddress(from, fromName);
            message.Subject = subject;
            message.Html = body;

            return message;
        }
    }
}