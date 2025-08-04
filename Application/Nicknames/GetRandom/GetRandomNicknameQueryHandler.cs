using Application.UserProfiles.Nickname;
using MediatR;

namespace Application.Nicknames.GetRandom
{
    public class GetRandomNicknameQueryHandler(INicknameGenerator nicknameGenerator) : IRequestHandler<GetRandomNicknameQuery, GetRandomNicknameResponse>
    {
        public async Task<GetRandomNicknameResponse> Handle(GetRandomNicknameQuery request, CancellationToken cancellationToken)
        {
            string nickname = await nicknameGenerator.GenerateUniqueNicknameAsync(cancellationToken);
            return new GetRandomNicknameResponse(nickname);
        }
    }
}