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

            builder.HasKey(sq => sq.SeasonalQuestId);

            builder.Property(sq => sq.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sq => sq.Description)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(sq => sq.ActiveFrom)
                .IsRequired();

            builder.Property(sq => sq.ActiveTo)
                .IsRequired();

            builder.Property(sq => sq.IsActive)
                .HasComputedColumnSql("CASE WHEN GETUTCDATE() BETWEEN ActiveFrom AND ActiveTo THEN 1 ELSE 0 END");

            builder.Property(sq => sq.Emoji)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(q => q.IsImportant)
                .HasDefaultValue(false);
        }
    }
}
