using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class EmailService(EmailSendingUtil emailSendingUtil, IConfiguration config)
{
    public virtual void SendConfirmationEmail(string to, string token)
    {
        var address = config.GetValue<string>("FrontendURLs:MailConfirmationURL") + token;
        var body = "Please confirm your email address by clicking this button: <a target='blank' href='" +
                   address + "'>Confirm</a>";

        Task.Run(async () =>
            await emailSendingUtil.SendMail(to, "Confirmation email", body));
    }
}