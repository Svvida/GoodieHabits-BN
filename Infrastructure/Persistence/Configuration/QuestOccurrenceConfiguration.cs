using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestOccurrenceConfiguration : IEntityTypeConfiguration<QuestOccurrence>
    {
        public void Configure(EntityTypeBuilder<QuestOccurrence> builder)
        {
            builder.ToTable("QuestOccurrences");

            builder.HasKey(qo => qo.Id);
            builder.HasIndex(qo => qo.QuestId);
            builder.HasIndex(qo => qo.CompletedAt);

            // Analytics read path: "all periods for these quests between two dates". Unique because
            // PeriodStart alone identifies a period — this is what makes a duplicate structurally impossible.
            builder.HasIndex(qo => new { qo.QuestId, qo.PeriodStart })
                .IsUnique();

            builder.Property(qo => qo.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(qo => qo.QuestId)
                .IsRequired();

            // SQL `date` — a calendar fact, deliberately not a UTC instant.
            builder.Property(qo => qo.PeriodStart)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(qo => qo.PeriodEnd)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(qo => qo.TargetAmount)
                .HasColumnType("decimal(9,2)")
                .HasDefaultValue(1m)
                .IsRequired();

            builder.Property(qo => qo.Progress)
                .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(qo => qo.CompletedAt)
                .IsRequired(false);

            builder.Property(qo => qo.IsBackfilled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(qo => qo.RewardGrantedAt)
                .IsRequired(false);

            builder.Property(qo => qo.XpAwarded)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qo => qo.CoinsAwarded)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qo => qo.SkippedAt)
                .IsRequired(false);

            builder.Property(qo => qo.SkipReason)
                .HasMaxLength(250)
                .IsRequired(false);

            // Progress is denormalized, so concurrent taps on the same period must not both win.
            builder.Property(qo => qo.RowVersion)
                .IsRowVersion();

            builder.HasOne(qo => qo.Quest)
                .WithMany(q => q.QuestOccurrences)
                .HasForeignKey(qo => qo.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
