using Infrastructure.Persistence.Seeders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(AppDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding.");

            try
            {
                    _logger.LogInformation("No accounts found in the database. Generating new accounts.");
                    var accounts = AccountSeeder.GenerateAccounts(10);
                    _logger.LogInformation("Generated {Count} accounts.", accounts.Count);

                    await _context.Accounts.AddRangeAsync(accounts);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved accounts to the database.");

                    _logger.LogInformation("Generating One-Time Quests.");
                    var oneTimeQuests = OneTimeQuestSeeder.GenerateOneTimeQuests(50, accounts);
                    _logger.LogInformation("Generated {Count} One-Time Quests.", oneTimeQuests.Count);

                    await _context.OneTimeQuests.AddRangeAsync(oneTimeQuests);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved One-Time Quests to the database.");

                    // Uncomment these blocks as needed to test other seeders
                    /*
                    _logger.LogInformation("Generating Recurring Quests.");
                    var recurringQuests = RecurringQuestSeeder.GenerateRecurringQuests(20, accounts);
                    _logger.LogInformation("Generated {Count} Recurring Quests.", recurringQuests.Count);

                    await _context.RecurringQuests.AddRangeAsync(recurringQuests);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved Recurring Quests to the database.");

                    _logger.LogInformation("Generating Seasonal Quests.");
                    var seasonalQuests = SeasonalQuestSeeder.GenerateSeasonalQuests(5);
                    _logger.LogInformation("Generated {Count} Seasonal Quests.", seasonalQuests.Count);

                    await _context.SeasonalQuests.AddRangeAsync(seasonalQuests);

                    _logger.LogInformation("Generating User-Seasonal Quests.");
                    var userSeasonalQuests = UserSeasonalQuestSeeder.GenerateUserSeasonalQuests(30, accounts, seasonalQuests);
                    _logger.LogInformation("Generated {Count} User-Seasonal Quests.", userSeasonalQuests.Count);

                    await _context.UserSeasonalQuests.AddRangeAsync(userSeasonalQuests);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved User-Seasonal Quests to the database.");
                    */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database seeding.");
                throw; // Re-throw to ensure errors are not swallowed
            }

            _logger.LogInformation("Database seeding completed.");
        }
    }
}
