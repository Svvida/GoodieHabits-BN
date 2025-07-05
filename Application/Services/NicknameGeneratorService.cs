using Application.Helpers;
using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Services
{
    public class NicknameGeneratorService : INicknameGeneratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _random = new();

        public NicknameGeneratorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateUniqueNicknameAsync(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < 10; i++)
            {
                var nickname = GenerateRandomNickname();
                if (!await _unitOfWork.UserProfiles.DoesNicknameExistAsync(nickname, cancellationToken).ConfigureAwait(false))
                {
                    return nickname;
                }
            }

            return string.Concat(GenerateRandomNickname(), Guid.NewGuid().ToString("N").AsSpan(0, 6)); // Fallback if all attempts fail
        }

        private string GenerateRandomNickname()
        {
            var adjective = WordLists.Adjectives[_random.Next(WordLists.Adjectives.Count)];
            var noun = WordLists.Nouns[_random.Next(WordLists.Nouns.Count)];
            var number = _random.Next(1, 9999);

            return $"{adjective}{noun}{number}";
        }
    }
}
