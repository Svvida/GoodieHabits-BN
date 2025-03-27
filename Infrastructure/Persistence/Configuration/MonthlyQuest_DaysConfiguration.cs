using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class MonthlyQuest_DaysConfiguration : IEntityTypeConfiguration<MonthlyQuest_Days>
    {
        public void Configure(EntityTypeBuilder<MonthlyQuest_Days> builder)
        {
            builder.ToTable("MonthlyQuest_Days");

            builder.HasKey(mqd => mqd.Id);

            builder.HasIndex(mqd => mqd.QuestId);

            builder.Property(mqd => mqd.StartDay)
                .IsRequired();

            builder.Property(mqd => mqd.EndDay)
                .IsRequired();

            builder.HasOne(mqd => mqd.Quest)
                .WithOne(q => q.MonthlyQuest_Days)
                .HasForeignKey<MonthlyQuest_Days>(mqd => mqd.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
