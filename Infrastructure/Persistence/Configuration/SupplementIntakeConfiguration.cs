using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class SupplementIntakeConfiguration : IEntityTypeConfiguration<SupplementIntake>
    {
        public void Configure(EntityTypeBuilder<SupplementIntake> builder)
        {
            builder.ToTable("SupplementIntakes");
            builder.HasKey(i => i.Id);

            builder.HasIndex(i => new { i.UserProfileId, i.TakenOn });
            builder.HasIndex(i => i.SupplementId);
            builder.HasIndex(i => i.WorkoutSessionId);

            // ⚠️ Load-bearing. One row per (slot, day) makes double-ticking the same dose structurally
            // impossible — the same guarantee UNIQUE (QuestId, PeriodStart) gives quest occurrences, added up
            // front this time instead of after a repair migration. The filter is required: SQL Server treats
            // NULLs as equal in a unique index, which would otherwise collapse every ad-hoc intake on a day
            // into one row.
            builder.HasIndex(i => new { i.ScheduleSlotId, i.TakenOn })
                .IsUnique()
                .HasFilter("[ScheduleSlotId] IS NOT NULL")
                .HasDatabaseName("IX_SupplementIntakes_ScheduleSlotId_TakenOn");

            builder.Ignore(i => i.IsAdHoc);

            // SQL `date` — which day the dose was taken is a calendar fact and must not move with the timezone.
            builder.Property(i => i.TakenOn)
                .HasColumnType("date")
                .IsRequired();

            // A genuine instant, unlike TakenOn.
            builder.Property(i => i.TakenAt)
                .IsRequired();

            builder.Property(i => i.Amount)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.HasOne(i => i.UserProfile)
                .WithMany(u => u.SupplementIntakes)
                .HasForeignKey(i => i.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Restrict: a supplement with logged intakes cannot be deleted (deactivating is the retire path),
            // so the history of what was actually taken can never be erased by a catalog edit.
            builder.HasOne(i => i.Supplement)
                .WithMany(s => s.Intakes)
                .HasForeignKey(i => i.SupplementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict, with the slot delete handler clearing the FK: the dose was still taken, it just stops
            // belonging to a plan and becomes an ad-hoc row.
            builder.HasOne(i => i.ScheduleSlot)
                .WithMany(s => s.Intakes)
                .HasForeignKey(i => i.ScheduleSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // SetNull rather than Restrict: deleting a training session must not be blocked by, or destroy, the
            // supplements taken during it. The link is context, not ownership — it is the only coupling between
            // the supplements and workouts modules.
            builder.HasOne(i => i.WorkoutSession)
                .WithMany(s => s.SupplementIntakes)
                .HasForeignKey(i => i.WorkoutSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
