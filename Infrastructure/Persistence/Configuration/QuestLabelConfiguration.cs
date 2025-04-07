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
                .IsRequired()
                .HasMaxLength(25)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            builder.Property(ql => ql.BackgroundColor)
                .IsRequired()
                .HasMaxLength(7)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            builder.Property(ql => ql.TextColor)
                .IsRequired()
                .HasMaxLength(7)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            builder.HasOne(ql => ql.Account)
                .WithMany(a => a.Labels)
                .HasForeignKey(ql => ql.AccountId)
                .OnDelete(DeleteBehavior.NoAction); // Perform manual delete in repository when deleting account
        }
    }
}
