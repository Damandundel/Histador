namespace Histador.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    /// <summary>Development stand-in: writes emails (e.g. password reset links) to the console log.</summary>
    public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
    {
        public Task SendAsync(string to, string subject, string htmlBody)
        {
            logger.LogWarning("EMAIL to {To} | {Subject}\n{Body}", to, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
