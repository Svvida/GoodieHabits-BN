namespace Application.Common.Interfaces.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
