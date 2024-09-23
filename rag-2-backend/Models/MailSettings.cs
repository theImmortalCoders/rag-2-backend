namespace rag_2_backend.Models;

public class MailSettings
{
    public required string EmailId { get; init; }
    public required string Name { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required bool UseSSL { get; init; }
}