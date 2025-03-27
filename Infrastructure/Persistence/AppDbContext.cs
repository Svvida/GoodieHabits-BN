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
        public DbSet<Quest> Quests { get; set; }
        public DbSet<MonthlyQuest_Days> MonthlyQuest_Days { get; set; }
        public DbSet<WeeklyQuest_Day> WeeklyQuest_Days { get; set; }
        public DbSet<SeasonalQuest_Season> SeasonalQuest_Seasons { get; set; }
        public DbSet<QuestLabel> QuestLabels { get; set; }
        public DbSet<Quest_QuestLabel> Quest_QuestLabels { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

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
            modelBuilder.ApplyConfiguration(new MonthlyQuest_DaysConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new Quest_QuestLabelConfiguration());
            modelBuilder.ApplyConfiguration(new QuestConfiguration());
            modelBuilder.ApplyConfiguration(new QuestLabelConfiguration());
            modelBuilder.ApplyConfiguration(new SeasonalQuest_SeasonConfiguration());
            modelBuilder.ApplyConfiguration(new WeeklyQuest_DaysConfiguration());
        }
    }
}
