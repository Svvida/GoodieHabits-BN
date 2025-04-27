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
            builder.HasIndex(p => p.AccountId).IsUnique();
            builder.HasIndex(p => p.Nickname).IsUnique();

            builder.Property(p => p.Nickname)
                .IsRequired(false)
                .HasMaxLength(16);

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

            builder.Property(p => p.CompletedGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ExpiredGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.AbandonedGoals)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Bio)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(p => p.Account)
                .WithOne(a => a.Profile)
                .HasForeignKey<UserProfile>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
