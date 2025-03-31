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
    }
}
