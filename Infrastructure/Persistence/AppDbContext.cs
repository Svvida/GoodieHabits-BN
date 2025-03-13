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
        public DbSet<QuestMetadata> QuestsMetadata { get; set; }
        public DbSet<OneTimeQuest> OneTimeQuests { get; set; }
        public DbSet<DailyQuest> DailyQuests { get; set; }
        public DbSet<WeeklyQuest> WeeklyQuests { get; set; }
        public DbSet<MonthlyQuest> MonthlyQuests { get; set; }
        public DbSet<SeasonalQuest> SeasonalQuests { get; set; }
        public DbSet<QuestLabel> QuestLabels { get; set; }
        public DbSet<QuestMetadata_QuestLabel> QuestMetadata_QuestLabels { get; set; }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
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
            modelBuilder.ApplyConfiguration(new SeasonalQuestConfiguration());
            modelBuilder.ApplyConfiguration(new QuestMetadataConfiguration());
            modelBuilder.ApplyConfiguration(new DailyQuestConfiguration());
            modelBuilder.ApplyConfiguration(new WeeklyQuestConfiguration());
            modelBuilder.ApplyConfiguration(new MonthlyQuestConfiguration());
            modelBuilder.ApplyConfiguration(new QuestLabelConfiguration());
            modelBuilder.ApplyConfiguration(new QuestMetadata_QuestLabelConfiguration());
        }
    }
}
