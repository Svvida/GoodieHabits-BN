using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class OneTimeQuestConfiguration : IEntityTypeConfiguration<OneTimeQuest>
    {
        public void Configure(EntityTypeBuilder<OneTimeQuest> builder)
        {
            builder.ToTable("One_Time_Quests");

            builder.HasKey(otq => otq.Id);

            builder.Property(otq => otq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(otq => otq.Description)
                .IsRequired(false)
                .HasMaxLength(10000);

            builder.Property(otq => otq.StartDate)
                .IsRequired(false);

            builder.Property(otq => otq.EndDate)
                .IsRequired(false);

            builder.Property(otq => otq.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(otq => otq.IsCompleted)
                .IsRequired();

            builder.Property(otq => otq.Priority)
                .IsRequired(false);

            builder.Property(otq => otq.CreatedAt)
                .IsRequired();

            builder.Property(otq => otq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(otq => otq.QuestMetadata)
                .WithOne(qm => qm.OneTimeQuest)
                .HasForeignKey<OneTimeQuest>(otq => otq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
