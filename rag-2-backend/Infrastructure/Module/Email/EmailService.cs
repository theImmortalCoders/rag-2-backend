namespace rag_2_backend.Infrastructure.Module.Email;

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

    public virtual void SendPasswordResetMail(string to, string token)
    {
        var address = config.GetValue<string>("FrontendURLs:PasswordResetURL") + token;
        var body = "Reset your password by clicking this button: <a target='blank' href='" +
                   address + "'>Reset</a>";

        Task.Run(async () =>
            await emailSendingUtil.SendMail(to, "Password reset", body));
    }
}