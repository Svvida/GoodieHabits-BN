using Application.Dtos.UserProfile;
using MediatR;

namespace Application.UserProfiles.Nickname
{
    public class GetRandomNicknameQueryHandler(INicknameGenerator nicknameGenerator) : IRequestHandler<GetRandomNicknameQuery, GetNicknameDto>
    {
        private readonly INicknameGenerator _nicknameGenerator = nicknameGenerator;

        public async Task<GetNicknameDto> Handle(GetRandomNicknameQuery request, CancellationToken cancellationToken)
        {
            string nickname = await _nicknameGenerator.GenerateUniqueNicknameAsync(cancellationToken);
            return new GetNicknameDto { Nickname = nickname };
        }
    }
}