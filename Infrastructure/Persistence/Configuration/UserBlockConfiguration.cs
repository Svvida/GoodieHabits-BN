using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
    {
        public void Configure(EntityTypeBuilder<UserBlock> builder)
        {
            builder.ToTable("UserBlocks");

            builder.HasKey(ub => new { ub.BlockerUserProfileId, ub.BlockedUserProfileId });
            builder.HasIndex(ub => new { ub.BlockerUserProfileId, ub.BlockedUserProfileId }).IsUnique();

            builder.Property(ub => ub.BlockerUserProfileId)
                .IsRequired();
            builder.Property(ub => ub.BlockedUserProfileId)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(ub => ub.BlockerUserProfile)
                .WithMany(up => up.SentBlocks)
                .HasForeignKey(ub => ub.BlockerUserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ub => ub.BlockedUserProfile)
                .WithMany(up => up.ReceivedBlocks)
                .HasForeignKey(ub => ub.BlockedUserProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
