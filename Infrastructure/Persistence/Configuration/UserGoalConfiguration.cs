using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
    {
        public void Configure(EntityTypeBuilder<UserGoal> builder)
        {
            builder.ToTable("UserGoals");
            builder.HasKey(ug => ug.Id);
            builder.HasIndex(ug => ug.QuestId);
            builder.HasIndex(ug => new { ug.UserProfileId, ug.IsExpired, ug.QuestId });
            builder.HasIndex(ug => new { ug.UserProfileId, ug.IsAchieved, ug.IsExpired });
            builder.HasIndex(ug => ug.EndsAt);

            builder.Property(ug => ug.GoalType)
                .IsRequired();

            builder.Property(ug => ug.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ug => ug.EndsAt)
                .IsRequired();

            builder.Property(ug => ug.IsAchieved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ug => ug.AchievedAt)
                .IsRequired(false)
                .HasDefaultValue(null);

            builder.Property(ug => ug.IsExpired)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ug => ug.XpBonus)
                .IsRequired()
                .HasDefaultValue(5);

            builder.HasOne(ug => ug.Quest)
                .WithMany(q => q.UserGoal)
                .HasForeignKey(ug => ug.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ug => ug.UserProfile)
                .WithMany(a => a.UserGoals)
                .HasForeignKey(ug => ug.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction); // Goals are deleted from Quest deletion
        }
    }
}
