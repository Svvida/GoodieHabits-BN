using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<MonthlyQuest_Days> MonthlyQuest_Days { get; set; }
        public DbSet<WeeklyQuest_Day> WeeklyQuest_Days { get; set; }
        public DbSet<SeasonalQuest_Season> SeasonalQuest_Seasons { get; set; }
        public DbSet<QuestLabel> QuestLabels { get; set; }
        public DbSet<Quest_QuestLabel> Quest_QuestLabels { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserProfile_Badge> UserProfile_Badges { get; set; }
        public DbSet<UserGoal> UserGoals { get; set; }
        public DbSet<QuestStatistics> QuestStatistics { get; set; }
        public DbSet<QuestOccurrence> QuestOccurrences { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FriendInvitation> FriendInvitations { get; set; }
        public DbSet<UserBlock> UserBlocks { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<ActiveUserEffect> ActiveUserEffects { get; set; }

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
            modelBuilder.ApplyConfiguration(new BadgeConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfile_BadgeConfiguration());
            modelBuilder.ApplyConfiguration(new UserGoalConfiguration());
            modelBuilder.ApplyConfiguration(new QuestStatisticsConfiguration());
            modelBuilder.ApplyConfiguration(new QuestOccurrenceConfiguration());
            modelBuilder.ApplyConfiguration(new FriendInvitationConfiguration());
            modelBuilder.ApplyConfiguration(new UserBlockConfiguration());
            modelBuilder.ApplyConfiguration(new FriendshipConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopItemConfiguration());
            modelBuilder.ApplyConfiguration(new UserInventoryConfiguration());
            modelBuilder.ApplyConfiguration(new ActiveUserEffectConfiguration());

            modelBuilder.Entity<Badge>().HasData(
                new Badge(1, BadgeTypeEnum.CompleteDailySeven, "Daily Streak: 7", "One week of daily quests in a row!", "#008000"),
                new Badge(2, BadgeTypeEnum.CompleteDailyThirty, "Daily Streak: 30", "A whole month of consistency. The fire never went out.", "#008000"),
                new Badge(3, BadgeTypeEnum.CompleteMonthlyTwelve, "Cycle Breaker", "Completed a monthly quest every month for a full year.", "#008000"),
                new Badge(4, BadgeTypeEnum.Complete500Quests, "Quest Master", "500 quests completed. Legendary endurance!", "#008000"),

                new Badge(5, BadgeTypeEnum.GoalCreateFirst, "Dreamer", "You set your very first goal.", "#ffbf00"),
                new Badge(6, BadgeTypeEnum.GoalCreateTen, "Planner", "You drafted 10 goals to challenge yourself.", "#ffbf00"),
                new Badge(7, BadgeTypeEnum.GoalCompleteFirst, "Achiever", "Your first goal completed. The journey begins.", "#ffbf00"),
                new Badge(8, BadgeTypeEnum.GoalCompleteTen, "Goal Getter", "10 goals done. Determination proven.", "#ffbf00"),
                new Badge(9, BadgeTypeEnum.GoalCompleteFifty, "Vision Realized", "50 goals achieved. You’ve come a long way.", "#ffbf00"),
                new Badge(10, BadgeTypeEnum.GoalCompleteYearly, "Yearly Champion", "Completed a yearly goal. True perseverance.", "#ffd700"),

                new Badge(11, BadgeTypeEnum.CreateFirstQuest, "First Quest!", "Your adventure begins with the very first quest.", "#0000ff"),
                new Badge(12, BadgeTypeEnum.CreateTwentyQuests, "Quest Scribe", "Twenty quests written into your story.", "#0000ff"),
                new Badge(13, BadgeTypeEnum.Create100Quests, "Quest Factory", "100 quests forged. A true architect of challenges.", "#0000ff"),

                new Badge(14, BadgeTypeEnum.MakeOneFriend, "Ally Found", "Your first companion on the journey.", "#6a0dad"),
                new Badge(15, BadgeTypeEnum.MakeFiveFriends, "Band of Allies", "Five allies joined your party.", "#6a0dad"),
                new Badge(16, BadgeTypeEnum.MakeTenFriends, "Social Starter", "Ten friends along the way.", "#6a0dad"),
                new Badge(17, BadgeTypeEnum.MakeTwentyFriends, "Party Leader", "Twenty friends — your guild thrives.", "#6a0dad"),

                new Badge(18, BadgeTypeEnum.FailureComeback, "Phoenix", "Failed 10 times, but always rose again.", "#ff0000"),
                new Badge(19, BadgeTypeEnum.StreakRecovery, "Gritty Hero", "Lost your streak, but started again. That’s true grit.", "#ff0000"),
                new Badge(20, BadgeTypeEnum.BalancedHero, "Balanced Hero", "Completed 10 daily, 10 weekly, and 10 monthly quests.", "#c0c0c0"));
        }
    }
}
