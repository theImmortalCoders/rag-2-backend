#region

using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.Email;

public class EmailService(EmailSendingUtil emailSendingUtil, IConfiguration config)
{
    public virtual void SendConfirmationEmail(string to, string token)
    {
        var address = config.GetValue<string>("FrontendURLs:MailConfirmationURL") + token;
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "confirmation.html");
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Templates/Images", "rag-2.png");
        var body = File.ReadAllText(templatePath).Replace("{{address}}", address);

        Task.Run(async () =>
            await emailSendingUtil.SendMail(to, "Confirmation email", body, logoPath));
    }

    public virtual void SendPasswordResetMail(string to, string token)
    {
        var address = config.GetValue<string>("FrontendURLs:PasswordResetURL") + token;
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "password-reset.html");
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Templates/Images", "rag-2.png");
        var body = File.ReadAllText(templatePath).Replace("{{address}}", address);

        Task.Run(async () =>
            await emailSendingUtil.SendMail(to, "Password reset", body, logoPath));
    }
}