using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestConfiguration : IEntityTypeConfiguration<Quest>
    {
        public void Configure(EntityTypeBuilder<Quest> builder)
        {
            builder.ToTable("Quests");

            builder.HasKey(q => q.Id);

            builder.HasIndex(q => q.UserProfileId);

            builder.Property(q => q.UserProfileId)
                .IsRequired();

            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(q => q.Description)
                .IsRequired(false)
                .HasMaxLength(10000);

            builder.Property(q => q.Priority)
                .IsRequired(false);

            // SQL `date` — the quest's active range is a calendar fact, not an instant.
            builder.Property(q => q.StartDate)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(q => q.EndDate)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(q => q.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(q => q.LastCompletedAt)
                .IsRequired(false);

            builder.Property(q => q.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(q => q.UpdatedAt)
                .IsRequired(false);

            builder.Property(q => q.WasEverCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(q => q.Difficulty)
                .IsRequired(false);

            builder.Property(q => q.ScheduledTime)
                .IsRequired(false);

            builder.Property(q => q.DurationMinutes)
                .IsRequired(false);

            ConfigureSchedule(builder);
            ConfigureTarget(builder);

            builder.HasOne(q => q.UserProfile)
                .WithMany(a => a.Quests)
                .HasForeignKey(q => q.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// The schedule is owned, so it lives as columns on Quests rather than in a table of its own — it has
        /// no identity apart from the quest, and every read of a quest needs it. This is what replaces the
        /// <c>QuestType</c> discriminator and the three satellite tables that hung off it.
        /// </summary>
        private static void ConfigureSchedule(EntityTypeBuilder<Quest> builder)
        {
            builder.OwnsOne(q => q.Schedule, schedule =>
            {
                schedule.Property(s => s.Unit)
                    .HasColumnName("Schedule_Unit")
                    .HasConversion<string>()
                    .HasMaxLength(10)
                    .IsRequired();

                schedule.Property(s => s.Interval)
                    .HasColumnName("Schedule_Interval")
                    .HasDefaultValue(1)
                    .IsRequired();

                // Stored as the raw flags int: a set of weekdays is one value, not a child collection.
                schedule.Property(s => s.Weekdays)
                    .HasColumnName("Schedule_Weekdays")
                    .HasConversion<int?>()
                    .IsRequired(false);

                schedule.Property(s => s.MonthWindowStartDay)
                    .HasColumnName("Schedule_MonthWindowStartDay")
                    .IsRequired(false);

                schedule.Property(s => s.MonthWindowEndDay)
                    .HasColumnName("Schedule_MonthWindowEndDay")
                    .IsRequired(false);

                schedule.Property(s => s.YearWindowStart)
                    .HasColumnName("Schedule_YearWindowStart")
                    .IsRequired(false);

                schedule.Property(s => s.YearWindowEnd)
                    .HasColumnName("Schedule_YearWindowEnd")
                    .IsRequired(false);

                // Declared on the owned builder, not the owner: the column lives in the Quests table but EF
                // models it on the owned type, so an index spanning it and UserProfileId cannot be expressed.
                // UserProfileId keeps its own index, which is what actually narrows these queries.
                schedule.HasIndex(s => s.Unit);
            });

            builder.Navigation(q => q.Schedule).IsRequired();
        }

        private static void ConfigureTarget(EntityTypeBuilder<Quest> builder)
        {
            builder.OwnsOne(q => q.Target, target =>
            {
                target.Property(t => t.Amount)
                    .HasColumnName("Target_Amount")
                    .HasColumnType("decimal(9,2)")
                    .HasDefaultValue(1m)
                    .IsRequired();

                target.Property(t => t.Unit)
                    .HasColumnName("Target_Unit")
                    .HasMaxLength(QuestTarget.UnitMaxLength)
                    .IsRequired(false);

                target.Property(t => t.Mode)
                    .HasColumnName("Target_Mode")
                    .HasConversion<string>()
                    .HasMaxLength(10)
                    .HasDefaultValue(TargetModeEnum.AtLeast)
                    .IsRequired();

                target.Property(t => t.MaxCompletionsPerDay)
                    .HasColumnName("Target_MaxCompletionsPerDay")
                    .IsRequired(false);
            });

            builder.Navigation(q => q.Target).IsRequired();
        }
    }
}
