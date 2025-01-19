using Bogus;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public static class OneTimeQuestSeeder
    {
        public static List<OneTimeQuest> GenerateOneTimeQuests(int count, List<Account> accounts)
        {
            var emojis = new[] { "🔥", "💧", "🌟", "🌿", "⚡", "💎", "🎯", "🏆" };

            var faker = new Faker<OneTimeQuest>()
                .RuleFor(q => q.AccountId, f => f.PickRandom(accounts).AccountId)
                .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
                .RuleFor(q => q.Description, f => f.Lorem.Paragraph())
                .RuleFor(q => q.EndDate, f => f.Date.Past(1))
                .RuleFor(q => q.EndDate, f => f.Date.Future(1))
                .RuleFor(q => q.IsImportant, f => f.Random.Bool())
                .RuleFor(q => q.Emoji, f => f.PickRandom(emojis));

            return faker.Generate(count);
        }
    }
}
