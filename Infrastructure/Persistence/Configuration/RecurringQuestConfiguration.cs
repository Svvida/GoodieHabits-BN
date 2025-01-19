using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class RecurringQuestConfiguration : IEntityTypeConfiguration<RecurringQuest>
    {
        public void Configure(EntityTypeBuilder<RecurringQuest> builder)
        {
            builder.ToTable("Recurring_Quests");

            builder.HasKey(rq => rq.RecurringQuestId);

            builder.Property(rq => rq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(rq => rq.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(rq => rq.Emoji)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(q => q.IsImportant)
                .HasDefaultValue(false);

            builder.Property(otq => otq.StartDate)
                .IsRequired(false);

            builder.Property(otq => otq.EndDate)
                .IsRequired(false);

            builder.Property(rq => rq.RepeatTime)
                .IsRequired();

            builder.Property(rq => rq.RepeatIntervalJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)"); // Store JSON data

            builder.HasOne(rq => rq.Account)
                .WithMany(a => a.RecurringQuests)
                .HasForeignKey(rq => rq.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
