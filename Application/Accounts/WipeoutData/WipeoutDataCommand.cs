using MediatR;

namespace Application.Accounts.WipeoutData
{
    public record WipeoutDataCommand(string Password, string ConfirmPassword, int AccountId) : IRequest;
}
