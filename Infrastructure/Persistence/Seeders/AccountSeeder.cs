using Bogus;
using Domain.Models;

namespace Infrastructure.Persistence.Seeders
{
    public static class AccountSeeder
    {
        public static List<Account> GenerateAccounts(int count)
        {
            var faker = new Faker<Account>()
                .RuleFor(a => a.Username, f => f.Internet.UserName())
                .RuleFor(a => a.HashPassword, f => f.Internet.Password())
                .RuleFor(a => a.Email, f => f.Internet.Email())
                .RuleFor(a => a.CreatedAt, f => f.Date.Past())
                .RuleFor(a => a.UpdatedAt, f => f.Date.Past());

            return faker.Generate(count);
        }
    }
}
