using System;
using System.Net;
using System.Net.Mail;

namespace WebGallery.Utilities
{
    public class Office365EmailHelper
    {
        public static void SendAsync(string to, string from, string fromName, string subject, string body)
        {
            try
            {
                var mailMessage = CreateMailMessage(to, from, fromName, subject, body);
                using (var client = new SmtpClient())
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(from, "");
                    client.Port = 587;
                    client.Host = "outlook.office365.com";
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;
                    client.SendMailAsync(mailMessage);
                }
            }
            catch
            {
            }
        }

        public static MailMessage CreateMailMessage(string to, string from, string fromName, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException($"Invalid argument: {nameof(to)}");
            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException($"Invalid argument: {nameof(from)}");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException($"Invalid argument: {nameof(subject)}");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException($"Invalid argument: {nameof(body)}");

            to = to.Trim().Trim(',').Replace(';', ',');
            from = from.Trim().Trim(',').Replace(';', ',');

            MailMessage mailMessage = null;
            if (to.Contains(",") || from.Contains(","))
            {
                mailMessage = new MailMessage(from, to);
            }
            else
            {
                var fromAddress = string.IsNullOrWhiteSpace(fromName) ? new MailAddress(from) : new MailAddress(from, fromName.Trim());
                var toAddress = new MailAddress(to);
                mailMessage = new MailMessage(fromAddress, toAddress);
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            return mailMessage;
        }
    }
}