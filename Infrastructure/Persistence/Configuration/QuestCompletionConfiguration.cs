using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestCompletionConfiguration : IEntityTypeConfiguration<QuestCompletion>
    {
        public void Configure(EntityTypeBuilder<QuestCompletion> builder)
        {
            builder.ToTable("QuestCompletions");

            builder.HasKey(qc => qc.Id);

            // The two read paths: one quest's history, and everything the user did in a date range.
            builder.HasIndex(qc => new { qc.QuestId, qc.CompletedOn });
            builder.HasIndex(qc => new { qc.UserProfileId, qc.CompletedOn });
            builder.HasIndex(qc => qc.OccurrenceId);

            // Filtered so the many rows with no idempotency key do not collide with each other.
            builder.HasIndex(qc => new { qc.QuestId, qc.ClientRequestId })
                .IsUnique()
                .HasFilter("[ClientRequestId] IS NOT NULL");

            builder.Property(qc => qc.QuestId)
                .IsRequired();

            builder.Property(qc => qc.UserProfileId)
                .IsRequired();

            builder.Property(qc => qc.OccurrenceId)
                .IsRequired(false);

            // SQL `date` — which local day this counts for is a calendar fact, and it is what makes
            // backfilling possible at all.
            builder.Property(qc => qc.CompletedOn)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(qc => qc.CompletedAt)
                .IsRequired();

            builder.Property(qc => qc.LocalTime)
                .IsRequired(false);

            builder.Property(qc => qc.Amount)
                .HasColumnType("decimal(9,2)")
                .HasDefaultValue(1m)
                .IsRequired();

            builder.Property(qc => qc.IsBackfilled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(qc => qc.Source)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(qc => qc.ClientRequestId)
                .IsRequired(false);

            builder.Property(qc => qc.Note)
                .HasMaxLength(QuestCompletion.NoteMaxLength)
                .IsRequired(false);

            builder.Property(qc => qc.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(qc => qc.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(qc => qc.Quest)
                .WithMany(q => q.Completions)
                .HasForeignKey(qc => qc.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qc => qc.UserProfile)
                .WithMany(up => up.QuestCompletions)
                .HasForeignKey(qc => qc.UserProfileId)
                // The quest already cascades from the profile; a second cascade path is a SQL Server error.
                .OnDelete(DeleteBehavior.NoAction);

            // A dropped period leaves its completions behind as off-schedule history rather than deleting
            // the record of what the user actually did.
            builder.HasOne(qc => qc.Occurrence)
                .WithMany(qo => qo.Completions)
                .HasForeignKey(qc => qc.OccurrenceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
