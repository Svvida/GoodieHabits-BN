using MediatR;

namespace Application.Accounts.Queries.GetWithProfile
{
    public record GetAccountWithProfileQuery(int AccountId) : IRequest<GetAccountWithProfileResponse>;
}