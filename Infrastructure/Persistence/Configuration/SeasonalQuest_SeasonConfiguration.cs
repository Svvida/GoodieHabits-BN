using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class SeasonalQuest_SeasonConfiguration : IEntityTypeConfiguration<SeasonalQuest_Season>
    {
        public void Configure(EntityTypeBuilder<SeasonalQuest_Season> builder)
        {
            builder.ToTable("SeasonalQuest_Seasons");

            builder.HasKey(sqs => sqs.Id);

            builder.HasIndex(sqs => sqs.QuestId);

            builder.Property(sqs => sqs.Season)
                .IsRequired();

            builder.HasOne(sqs => sqs.Quest)
                .WithOne(q => q.SeasonalQuest_Season)
                .HasForeignKey<SeasonalQuest_Season>(sqs => sqs.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
