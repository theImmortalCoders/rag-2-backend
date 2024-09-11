namespace rag_2_backend.Models;

public class MailSettings
{
    public string EmailId { get; set; }
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
}