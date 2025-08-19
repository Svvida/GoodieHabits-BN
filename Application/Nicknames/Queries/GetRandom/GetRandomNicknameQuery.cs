using Application.Common.Interfaces;

namespace Application.Nicknames.Queries.GetRandom
{
    public record GetRandomNicknameQuery() : IQuery<GetRandomNicknameResponse>;
}
