using MediatR;

namespace Application.Nicknames.GetRandom
{
    public record GetRandomNicknameQuery() : IRequest<GetRandomNicknameResponse>;
}
