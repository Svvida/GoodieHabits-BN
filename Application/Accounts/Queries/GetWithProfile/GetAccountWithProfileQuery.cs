using Application.Common.Interfaces;

namespace Application.Accounts.Queries.GetWithProfile
{
    public record GetAccountWithProfileQuery(int AccountId) : IQuery<GetAccountWithProfileResponse>;
}