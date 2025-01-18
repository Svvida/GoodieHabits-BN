using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class UserSeasonalQuestConfiguration : IEntityTypeConfiguration<UserSeasonalQuest>
    {
        public void Configure(EntityTypeBuilder<UserSeasonalQuest> builder)
        {
            builder.ToTable("User_Seasonal_Quests");

            builder.HasKey(usq => new { usq.AccountId, usq.SeasonalQuestId });

            builder.HasOne(usq => usq.Account)
                .WithMany(a => a.UserSeasonalQuests)
                .HasForeignKey(usq => usq.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(usq => usq.SeasonalQuest)
                .WithMany(sq => sq.UserSeasonalQuests)
                .HasForeignKey(usq => usq.SeasonalQuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
