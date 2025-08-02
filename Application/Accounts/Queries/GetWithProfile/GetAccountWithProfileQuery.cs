using Application.Dtos.Accounts;
using MediatR;

namespace Application.Accounts.Queries.GetWithProfile
{
    public record GetAccountWithProfileQuery(int AccountId) : IRequest<GetAccountWithProfileDto>;
}