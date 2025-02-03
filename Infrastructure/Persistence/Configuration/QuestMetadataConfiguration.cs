using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class QuestMetadataConfiguration : IEntityTypeConfiguration<QuestMetadata>
    {
        public void Configure(EntityTypeBuilder<QuestMetadata> builder)
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

            builder.HasOne(q => q.Account)
                .WithMany(a => a.Quests)
                .HasForeignKey(q => q.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
