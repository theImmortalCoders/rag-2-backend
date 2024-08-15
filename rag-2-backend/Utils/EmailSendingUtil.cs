using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace rag_2_backend.Utils;

public class EmailSendingUtil(IOptions<MailSettings> options)
{
    private readonly MailSettings _mailSettings = options.Value;

    public bool SendMail(string to, string subject, string body)
    {
        try
        {
            var emailMessage = new MimeMessage();
            var emailFrom = new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId);
            emailMessage.From.Add(emailFrom);
            var emailTo = new MailboxAddress(to, to);
            emailMessage.To.Add(emailTo);
            emailMessage.Subject = subject;
            var emailBodyBuilder = new BodyBuilder();
            emailBodyBuilder.TextBody = body;
            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            var mailClient = new SmtpClient();
            mailClient.Connect(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSSL);
            mailClient.Authenticate(_mailSettings.EmailId, _mailSettings.Password);
            mailClient.Send(emailMessage);
            mailClient.Disconnect(true);
            mailClient.Dispose();
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
}