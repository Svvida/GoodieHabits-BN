using Application.Common.Interfaces;

namespace Application.Accounts.Commands.RequestPasswordReset
{
    public record RequestPasswordResetCommand(string? Email) : ICommand;
}
