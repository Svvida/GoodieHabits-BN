using MediatR;

namespace Application.Nicknames.Queries.GetRandom
{
    public record GetRandomNicknameQuery() : IRequest<GetRandomNicknameResponse>;
}
