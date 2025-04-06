using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nickname)
                .IsRequired(false);

            builder.Property(p => p.Avatar)
                .IsRequired(false);

            builder.Property(p => p.TotalXp)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.TotalQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CompletedQuests)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Bio)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(p => p.Account)
                .WithOne(a => a.Profile)
                .HasForeignKey<UserProfile>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
