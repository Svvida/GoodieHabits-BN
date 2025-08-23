using Application.Common.Interfaces.Email;
using Infrastructure.Email.Templates;

namespace Infrastructure.Email.Senders
{
    public class ForgotPasswordEmailSender(IEmailSender emailSender) : IForgotPasswordEmailSender
    {
        public async Task SendForgotPasswordEmailAsync(string toEmail, string resetCode, CancellationToken cancellationToken = default)
        {
            var subject = "Password Reset Request";
            var body = PasswordResetTemplate.BuildPasswordResetEmailBody(resetCode, toEmail);
            await emailSender.SendEmailAsync(toEmail, subject, body, cancellationToken).ConfigureAwait(false);
        }
    }
}
