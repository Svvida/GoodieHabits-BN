using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class BadgeConfiguration : IEntityTypeConfiguration<Badge>
    {
        public void Configure(EntityTypeBuilder<Badge> builder)
        {
            builder.ToTable("Badges");

            builder.HasKey(b => b.Id);

            builder.HasIndex(b => b.Type).IsUnique();

            builder.Property(b => b.Type)
                .IsRequired();

            builder.Property(b => b.Text)
                .IsRequired()
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");
        }
    }
}
