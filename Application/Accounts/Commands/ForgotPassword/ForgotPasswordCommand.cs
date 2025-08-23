
using Application.Common.Interfaces;

namespace Application.Accounts.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : ICommand;
}
