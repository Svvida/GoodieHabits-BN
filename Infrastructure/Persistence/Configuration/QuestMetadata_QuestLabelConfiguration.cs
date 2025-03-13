using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestMetadata_QuestLabelConfiguration : IEntityTypeConfiguration<QuestMetadata_QuestLabel>
    {
        public void Configure(EntityTypeBuilder<QuestMetadata_QuestLabel> builder)
        {
            builder.ToTable("QuestMetadata_QuestLabel");
            builder.HasKey(qml => new { qml.QuestMetadataId, qml.QuestLabelId });

            builder.HasOne(qml => qml.QuestMetadata)
                .WithMany(qm => qm.QuestLabels)
                .HasForeignKey(qml => qml.QuestMetadataId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(qml => qml.QuestLabel)
                .WithMany(ql => ql.QuestMetadataRelations)
                .HasForeignKey(qml => qml.QuestLabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
