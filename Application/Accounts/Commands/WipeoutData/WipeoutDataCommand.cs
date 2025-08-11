using MediatR;

namespace Application.Accounts.Commands.WipeoutData
{
    public record WipeoutDataCommand(string Password, string ConfirmPassword, int AccountId) : IRequest;
}
