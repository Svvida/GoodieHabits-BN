using Domain.Models;

namespace Tests.Factories
{
    public static class AccountFactory
    {
        public static Account CreateAccount(int accountId, string email, string? initialHashedPassword = null, string TimeZone = "Etc/UTC")
        {
            return new Account
            {
                Id = accountId,
                HashPassword = initialHashedPassword ?? $"hashed_password_for_{accountId}",
                Email = email,
                TimeZone = TimeZone
            };
        }

        public static Account CreateAccountWithProfile(int accountId, string email, string? initialHashedPassword = null, string timeZone = "Etc/UTC")
        {
            return new Account
            {
                Id = accountId,
                HashPassword = initialHashedPassword ?? $"hashed_password_for_{accountId}",
                Email = email,
                TimeZone = timeZone,
                Profile = new UserProfile
                {
                    AccountId = accountId,
                    Nickname = "TestNickname",
                    Bio = "TestBio"
                }
            };
        }
    }
}
