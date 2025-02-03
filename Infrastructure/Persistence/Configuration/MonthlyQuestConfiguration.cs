using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class MonthlyQuestConfiguration : IEntityTypeConfiguration<MonthlyQuest>
    {
        public void Configure(EntityTypeBuilder<MonthlyQuest> builder)
        {
            builder.ToTable("Monthly_Quests");

            builder.HasKey(mq => mq.Id);

            builder.Property(mq => mq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(mq => mq.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(mq => mq.StartDate)
                .IsRequired(false);

            builder.Property(mq => mq.EndDate)
                .IsRequired(false);

            builder.Property(mq => mq.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(mq => mq.IsCompleted)
                .IsRequired();

            builder.Property(mq => mq.Priority)
                .IsRequired(false);

            builder.Property(mq => mq.StartDay)
                .IsRequired();

            builder.Property(mq => mq.EndDay)
                .IsRequired();

            builder.Property(mq => mq.CreatedAt)
                .IsRequired();

            builder.Property(mq => mq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(mq => mq.Quest)
                .WithOne()
                .HasForeignKey<MonthlyQuest>(mq => mq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
