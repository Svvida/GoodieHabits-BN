using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.Id);

            builder.HasIndex(a => a.Email).IsUnique();
            builder.HasIndex(a => a.Login).IsUnique();

            builder.Property(a => a.Login)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(a => a.HashPassword)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(email => email.ToLower(), email => email);

            builder.Property(a => a.TimeZone)
                .IsRequired()
                .HasDefaultValue("Etc/UTC")
                .HasMaxLength(32);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .IsRequired(false);
        }
    }
}
