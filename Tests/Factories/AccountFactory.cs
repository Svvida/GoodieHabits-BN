using Domain.Models;

namespace Tests.Factories
{
    public static class AccountFactory
    {
        public static Account CreateAccount(int accountId, string hashPassword, string email, string TimeZone)
        {
            return new Account(accountId, hashPassword, email, TimeZone)
            {
                Id = accountId,
                HashPassword = hashPassword,
                Email = email,
                TimeZone = TimeZone
            };
        }

        public static Account CreateAccountWithProfile(int accountId, string hashPassword, string email, string timeZone)
        {
            return new Account(accountId, hashPassword, email, timeZone)
            {
                Id = accountId,
                HashPassword = hashPassword,
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
