using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class UserProfile_BadgeConfiguration : IEntityTypeConfiguration<UserProfile_Badge>
    {
        public void Configure(EntityTypeBuilder<UserProfile_Badge> builder)
        {
            builder.ToTable("UserProfile_Badges");

            builder.HasKey(upb => new { upb.UserProfileId, upb.BadgeId });
            builder.HasIndex(upb => new { upb.UserProfileId, upb.EarnedAt });

            builder.Property(upb => upb.EarnedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(upb => upb.UserProfile)
                .WithMany(up => up.UserProfile_Badges)
                .HasForeignKey(upb => upb.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(upb => upb.Badge)
                .WithMany(b => b.UserProfile_Badges)
                .HasForeignKey(upb => upb.BadgeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of badges that are in use
        }
    }
}
