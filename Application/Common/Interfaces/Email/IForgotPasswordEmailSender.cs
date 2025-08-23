namespace Application.Common.Interfaces.Email
{
    public interface IForgotPasswordEmailSender
    {
        Task SendForgotPasswordEmailAsync(string toEmail, int resetCode, CancellationToken cancellationToken = default);
    }
}
