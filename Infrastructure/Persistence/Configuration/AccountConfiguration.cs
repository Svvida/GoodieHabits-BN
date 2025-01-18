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

            builder.HasKey(a => a.AccountId);

            builder.HasIndex(a => a.Email).IsUnique();
            builder.HasIndex(a => a.Username).IsUnique();

            builder.Property(a => a.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.HashPassword)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
