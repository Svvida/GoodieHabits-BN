using Application.Common.Interfaces;

namespace Application.Accounts.Commands.WipeoutData
{
    public record WipeoutDataCommand(string Password, string ConfirmPassword, int UserProfileId) : ICommand;
}
