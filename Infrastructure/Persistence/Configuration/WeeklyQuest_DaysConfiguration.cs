using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WeeklyQuest_DaysConfiguration : IEntityTypeConfiguration<WeeklyQuest_Day>
    {
        public void Configure(EntityTypeBuilder<WeeklyQuest_Day> builder)
        {
            builder.ToTable("WeeklyQuest_Days");

            builder.HasKey(wqd => wqd.Id);

            builder.HasIndex(wqd => wqd.QuestId);

            builder.Property(wqd => wqd.Weekday)
                .IsRequired();

            builder.HasOne(wqd => wqd.Quest)
                .WithMany(q => q.WeeklyQuest_Days)
                .HasForeignKey(wqd => wqd.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
