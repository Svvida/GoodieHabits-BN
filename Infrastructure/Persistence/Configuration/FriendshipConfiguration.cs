using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            builder.ToTable("Friendships");

            builder.HasKey(f => new { f.UserProfileId1, f.UserProfileId2 });
            builder.HasIndex(f => new { f.UserProfileId1, f.UserProfileId2 }).IsUnique();

            builder.Property(f => f.UserProfileId1)
                .IsRequired();

            builder.Property(f => f.UserProfileId2)
                .IsRequired();

            builder.HasOne(f => f.UserProfile1)
                .WithMany(up => up.FriendshipsAsUser1)
                .HasForeignKey(f => f.UserProfileId1)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.UserProfile2)
                .WithMany(up => up.FriendshipsAsUser2)
                .HasForeignKey(f => f.UserProfileId2)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
