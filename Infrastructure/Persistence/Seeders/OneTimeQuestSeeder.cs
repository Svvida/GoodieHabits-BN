using Bogus;
using Domain.Models;

namespace Infrastructure.Persistence.Seeders
{
    public static class OneTimeQuestSeeder
    {
        public static List<OneTimeQuest> GenerateOneTimeQuests(int count, List<Account> accounts)
        {
            var emojis = new[] { "🔥", "💧", "🌟", "🌿", "⚡", "💎", "🎯", "🏆" };

            var faker = new Faker<OneTimeQuest>()
                .RuleFor(q => q.Id, f => f.PickRandom(accounts).Id)
                .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
                .RuleFor(q => q.Description, f => f.Lorem.Paragraph())
                .RuleFor(q => q.EndDate, f => f.Date.Past(1))
                .RuleFor(q => q.EndDate, f => f.Date.Future(1))
                .RuleFor(q => q.Emoji, f => f.PickRandom(emojis));

            return faker.Generate(count);
        }
    }
}
