using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class DailyQuestConfiguration : IEntityTypeConfiguration<DailyQuest>
    {
        public void Configure(EntityTypeBuilder<DailyQuest> builder)
        {
            builder.ToTable("Daily_Quests");

            builder.HasKey(dq => dq.Id);

            builder.Property(dq => dq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(dq => dq.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(dq => dq.StartDate)
                .IsRequired(false);

            builder.Property(dq => dq.EndDate)
                .IsRequired(false);

            builder.Property(dq => dq.IsCompleted)
                .IsRequired();

            builder.Property(dq => dq.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(dq => dq.CreatedAt)
                .IsRequired();

            builder.Property(dq => dq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(dq => dq.Quest)
                .WithOne()
                .HasForeignKey<DailyQuest>(dq => dq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
