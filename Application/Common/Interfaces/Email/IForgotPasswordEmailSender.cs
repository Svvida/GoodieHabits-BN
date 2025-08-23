namespace Application.Common.Interfaces.Email
{
    public interface IForgotPasswordEmailSender
    {
        Task SendForgotPasswordEmailAsync(string toEmail, string resetCode, CancellationToken cancellationToken = default);
    }
}
