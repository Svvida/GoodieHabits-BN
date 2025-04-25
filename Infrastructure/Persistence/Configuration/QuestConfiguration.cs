using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestConfiguration : IEntityTypeConfiguration<Quest>
    {
        public void Configure(EntityTypeBuilder<Quest> builder)
        {
            builder.ToTable("Quests");

            builder.HasKey(q => q.Id);

            builder.HasIndex(q => q.QuestType);
            builder.HasIndex(q => q.AccountId);
            builder.HasIndex(q => new { q.AccountId, q.QuestType });

            builder.Property(q => q.QuestType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(q => q.AccountId)
                .IsRequired();

            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(dq => dq.Description)
                .IsRequired(false)
                .HasMaxLength(10000);

            builder.Property(q => q.Priority)
                .IsRequired(false);

            builder.Property(q => q.IsCompleted)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(q => q.StartDate)
                .IsRequired(false);

            builder.Property(q => q.EndDate)
                .IsRequired(false);

            builder.Property(q => q.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(q => q.LastCompletedAt)
                .IsRequired(false);

            builder.Property(q => q.NextResetAt)
                .IsRequired(false);

            builder.Property(q => q.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(dq => dq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(q => q.Account)
                .WithMany(a => a.Quests)
                .HasForeignKey(q => q.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
