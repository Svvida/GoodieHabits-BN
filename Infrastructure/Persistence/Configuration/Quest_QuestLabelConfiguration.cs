using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class Quest_QuestLabelConfiguration : IEntityTypeConfiguration<Quest_QuestLabel>
    {
        public void Configure(EntityTypeBuilder<Quest_QuestLabel> builder)
        {
            builder.ToTable("Quest_QuestLabel");
            builder.HasKey(qql => new { qql.QuestId, qql.QuestLabelId });

            builder.HasOne(qql => qql.Quest)
                .WithMany(q => q.Quest_QuestLabels)
                .HasForeignKey(qql => qql.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qql => qql.QuestLabel)
                .WithMany(ql => ql.Quest_QuestLabels)
                .HasForeignKey(qql => qql.QuestLabelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
