using Domain.Common;
using Domain.Models;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<OneTimeQuest> OneTimeQuests { get; set; }
        public DbSet<RecurringQuest> RecurringQuests { get; set; }
        public DbSet<SeasonalQuest> SeasonalQuests { get; set; }
        public DbSet<UserSeasonalQuest> UserSeasonalQuests { get; set; }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreatedAt(DateTime.UtcNow);
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.SetUpdatedAt(DateTime.UtcNow);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new OneTimeQuestConfiguration());
            modelBuilder.ApplyConfiguration(new RecurringQuestConfiguration());
            modelBuilder.ApplyConfiguration(new SeasonalQuestConfiguration());
            modelBuilder.ApplyConfiguration(new UserSeasonalQuestConfiguration());
        }
    }
}
