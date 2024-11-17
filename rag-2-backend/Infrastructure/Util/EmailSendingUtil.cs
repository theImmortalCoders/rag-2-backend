#region

using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Util;

public class EmailSendingUtil(IOptions<MailSettings> options)
{
    private readonly MailSettings _mailSettings = options.Value;

    public async Task<bool> SendMail(string to, string subject, string body, string? imagePath = null)
    {
        try
        {
            var emailMessage = new MimeMessage();
            var emailFrom = new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId);
            emailMessage.From.Add(emailFrom);
            var emailTo = new MailboxAddress(to, to);
            emailMessage.To.Add(emailTo);
            emailMessage.Subject = subject;
            var emailBodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            if (imagePath != null && File.Exists(imagePath))
            {
                var image = await emailBodyBuilder.LinkedResources.AddAsync(imagePath);
                image.ContentId = "logo";
            }

            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            using var mailClient = new SmtpClient();
            await mailClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSSL);
            await mailClient.AuthenticateAsync(_mailSettings.EmailId, _mailSettings.Password);
            await mailClient.SendAsync(emailMessage);
            await mailClient.DisconnectAsync(true);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}