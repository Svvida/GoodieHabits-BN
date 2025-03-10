using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class QuestLabelConfiguration : IEntityTypeConfiguration<QuestLabel>
    {
        public void Configure(EntityTypeBuilder<QuestLabel> builder)
        {
            builder.ToTable("Quest_Labels");
            builder.HasIndex(ql => ql.Value);

            builder.HasKey(ql => ql.Id);

            builder.Property(ql => ql.Value)
                .IsRequired();

            builder.Property(ql => ql.BackgroundColor)
                .IsRequired();

            builder.Property(ql => ql.TextColor)
                .IsRequired();

            builder.HasOne(ql => ql.Account)
                .WithMany(a => a.Labels)
                .HasForeignKey(ql => ql.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
