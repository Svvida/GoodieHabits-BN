using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class SeasonalQuestConfiguration : IEntityTypeConfiguration<SeasonalQuest>
    {
        public void Configure(EntityTypeBuilder<SeasonalQuest> builder)
        {
            builder.ToTable("Seasonal_Quests");

            builder.HasKey(sq => sq.Id);

            builder.Property(sq => sq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sq => sq.Description)
                .IsRequired(false)
                .HasMaxLength(10000);

            builder.Property(sq => sq.StartDate)
                .IsRequired(false);

            builder.Property(sq => sq.EndDate)
                .IsRequired(false);

            builder.Property(sq => sq.IsCompleted)
                .IsRequired();

            builder.Property(sq => sq.Emoji)
                .IsRequired(false)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(sq => sq.Season)
                .IsRequired();

            builder.Property(sq => sq.Priority)
                .IsRequired(false);

            builder.Property(sq => sq.CreatedAt)
                .IsRequired();

            builder.Property(sq => sq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(sq => sq.QuestMetadata)
                .WithOne(qm => qm.SeasonalQuest)
                .HasForeignKey<SeasonalQuest>(sq => sq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
