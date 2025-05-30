using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestStatisticsConfiguration : IEntityTypeConfiguration<QuestStatistics>
    {
        public void Configure(EntityTypeBuilder<QuestStatistics> builder)
        {
            builder.ToTable("QuestStatistics");
            builder.HasKey(qs => qs.Id);
            builder.HasIndex(qs => qs.QuestId).IsUnique();

            builder.Property(qs => qs.QuestId)
                .IsRequired();

            builder.Property(qs => qs.CompletionCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qs => qs.FailureCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qs => qs.OccurrenceCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qs => qs.CurrentStreak)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qs => qs.LongestStreak)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(qs => qs.LastCompletedAt)
                .IsRequired(false);

            builder.HasOne(qs => qs.Quest)
                .WithOne(q => q.Statistics)
                .HasForeignKey<QuestStatistics>(qs => qs.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
