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

            // Analytics read path: "all periods for these quests between two dates".
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

            builder.Property(qo => qo.WasCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(qo => qo.CompletedAt)
                .IsRequired(false);

            builder.Property(qo => qo.IsBackfilled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(qo => qo.Quest)
                .WithMany(q => q.QuestOccurrences)
                .HasForeignKey(qo => qo.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
