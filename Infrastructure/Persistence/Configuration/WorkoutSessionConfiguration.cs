using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
    {
        public void Configure(EntityTypeBuilder<WorkoutSession> builder)
        {
            builder.ToTable("WorkoutSessions");
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => new { s.UserProfileId, s.PerformedOn });
            builder.HasIndex(s => s.RoutineId);

            // At most one in-progress session per user, enforced by the database rather than by a read-then-write
            // race in the handler. Filtered on Status = 0 (InProgress).
            builder.HasIndex(s => s.UserProfileId)
                .IsUnique()
                .HasFilter("[Status] = 0")
                .HasDatabaseName("IX_WorkoutSessions_UserProfileId_ActiveOnly");

            builder.Ignore(s => s.IsInProgress);
            builder.Ignore(s => s.DurationSeconds);

            builder.Property(s => s.Name)
                .HasMaxLength(WorkoutSession.NameMaxLength)
                .IsRequired();

            // SQL `date` — the local day the user trained, deliberately not a UTC instant.
            builder.Property(s => s.PerformedOn)
                .HasColumnType("date")
                .IsRequired();

            // Genuine instants, unlike PerformedOn.
            builder.Property(s => s.StartedAt)
                .IsRequired();

            builder.Property(s => s.CompletedAt)
                .IsRequired(false);

            builder.Property(s => s.Status)
                .IsRequired();

            builder.Property(s => s.Note)
                .HasMaxLength(WorkoutSession.NoteMaxLength);

            builder.HasOne(s => s.UserProfile)
                .WithMany(u => u.WorkoutSessions)
                .HasForeignKey(s => s.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Provenance link to the template. Restrict, with the routine delete handler clearing the FK first:
            // a performed session is the user's own record and must outlive the routine it came from — the same
            // posture as FinanceTransaction.RecurringTransactionId.
            builder.HasOne(s => s.Routine)
                .WithMany()
                .HasForeignKey(s => s.RoutineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Exercises)
                .WithOne(e => e.WorkoutSession)
                .HasForeignKey(e => e.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
