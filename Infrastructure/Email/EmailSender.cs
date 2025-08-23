using Application.Common.Interfaces.Email;
using Domain.Exceptions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email
{
    public class EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger) : IEmailSender
    {
        private readonly EmailSettings _emailSettings = emailSettings.Value;

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("User", toEmail));

            message.Subject = subject;

            logger.LogInformation("Sending email to {ToEmail} with subject {Subject}", toEmail, subject);

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken).ConfigureAwait(false);

                await smtp.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword, cancellationToken).ConfigureAwait(false);

                await smtp.SendAsync(message, cancellationToken).ConfigureAwait(false);
                smtp.Disconnect(true, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw new FailedToSendEmailException(); // Throw general exception to avoid leaking sensitive info
            }
            finally
            {
                await smtp.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
                smtp.Dispose();
            }
        }
    }
}
