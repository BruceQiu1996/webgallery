using System.IO;
using System.Net.Mail;

namespace WebGallery.Utilities
{
    public class EmailHelper
    {
        public static void Send(string from, string fromName, string to, string subject, string body)
        {
            Send(from, fromName, to, subject, body, null, null);
        }

        public static void Send(string from, string fromName, string to, string subject, string body, Stream attachedFile, string attachedFileName)
        {
            if (string.IsNullOrWhiteSpace(to)) return;
            if (string.IsNullOrWhiteSpace(from)) return;
            if (string.IsNullOrWhiteSpace(subject)) return;
            if (string.IsNullOrWhiteSpace(body)) return;

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

            if ((attachedFile != null) && (attachedFileName != null))
            {
                attachedFile.Position = 0;
                var msgAttachment = new Attachment(attachedFile, attachedFileName.Trim());
                mailMessage.Attachments.Add(msgAttachment);
            }

            try
            {
                new SmtpClient().Send(mailMessage);
            }
            catch
            {
            }
        }
    }
}