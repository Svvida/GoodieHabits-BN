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
            builder.HasIndex(qo => qo.OccurrenceStart);
            builder.HasIndex(qo => qo.CompletedAt);
            builder.HasIndex(qo => new { qo.QuestId, qo.OccurrenceStart, qo.OccurrenceEnd })
                .IsUnique();

            builder.Property(qo => qo.QuestId)
                .IsRequired();

            builder.Property(qo => qo.OccurrenceStart)
                .IsRequired();

            builder.Property(qo => qo.OccurrenceEnd)
                .IsRequired();

            builder.Property(qo => qo.WasCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(qo => qo.CompletedAt)
                .IsRequired(false);

            builder.HasOne(qo => qo.Quest)
                .WithMany(q => q.QuestOccurrences)
                .HasForeignKey(qo => qo.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
