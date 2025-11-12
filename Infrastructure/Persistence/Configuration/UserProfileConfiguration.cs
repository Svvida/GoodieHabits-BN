using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.AccountId).IsUnique();
            builder.HasIndex(p => p.Nickname).IsUnique();

            builder.Property(a => a.TimeZone)
                .IsRequired()
                .HasDefaultValue("Etc/UTC")
                .HasMaxLength(100);

            builder.Property(p => p.Nickname)
                .IsRequired(true)
                .HasMaxLength(30);

            builder.Property(p => p.Avatar)
                .IsRequired(false);

            builder.Property(p => p.TotalXp)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.TotalQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedDailyQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedWeeklyQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedMonthlyQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ExistingQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CurrentlyCompletedExistingQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.EverCompletedExistingQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ExpiredGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.TotalGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ActiveGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.FriendsCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Bio)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(p => p.Account)
                .WithOne(a => a.Profile)
                .HasForeignKey<UserProfile>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
