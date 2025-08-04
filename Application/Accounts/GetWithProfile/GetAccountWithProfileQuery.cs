using MediatR;

namespace Application.Accounts.GetWithProfile
{
    public record GetAccountWithProfileQuery(int AccountId) : IRequest<GetAccountWithProfileResponse>;
}