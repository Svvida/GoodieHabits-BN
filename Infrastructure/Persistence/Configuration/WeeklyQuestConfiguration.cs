using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WeeklyQuestConfiguration : IEntityTypeConfiguration<WeeklyQuest>
    {
        public void Configure(EntityTypeBuilder<WeeklyQuest> builder)
        {
            builder.ToTable("Weekly_Quests");

            builder.HasKey(wq => wq.Id);

            builder.Property(wq => wq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(wq => wq.Description)
                .IsRequired(false)
                .HasMaxLength(10000);

            builder.Property(wq => wq.StartDate)
                .IsRequired(false);

            builder.Property(wq => wq.EndDate)
                .IsRequired(false);

            builder.Property(wq => wq.IsCompleted)
                .IsRequired();

            builder.Property(wq => wq.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(wq => wq.WeekdaysSerialized)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(wq => wq.CreatedAt)
                .IsRequired();

            builder.Property(wq => wq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(wq => wq.QuestMetadata)
                .WithOne(qm => qm.WeeklyQuest)
                .HasForeignKey<WeeklyQuest>(wq => wq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
