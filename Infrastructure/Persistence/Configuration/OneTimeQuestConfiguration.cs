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

            builder.HasKey(otq => otq.OneTimeQuestId);

            builder.Property(otq => otq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(otq => otq.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(otq => otq.StartDate)
                .IsRequired(false);

            builder.Property(otq => otq.EndDate)
                .IsRequired(false);

            builder.Property(otq => otq.Emoji)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.HasOne(otq => otq.Account)
                .WithMany(a => a.OneTimeQuests)
                .HasForeignKey(otq => otq.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
