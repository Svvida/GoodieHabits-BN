using Domain.Common;
using Domain.Interfaces;

namespace Application.UserProfiles.Nickname
{
    public class NicknameGenerator(IUnitOfWork unitOfWork) : INicknameGenerator
    {
        private readonly Random _random = new();

        public async Task<string> GenerateUniqueNicknameAsync(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < 10; i++)
            {
                var nickname = GenerateRandomNickname();
                if (!await unitOfWork.UserProfiles.DoesNicknameExistAsync(nickname, cancellationToken).ConfigureAwait(false))
                    return nickname;
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
