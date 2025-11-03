using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class FriendInvitationConfiguration : IEntityTypeConfiguration<FriendInvitation>
    {
        public void Configure(EntityTypeBuilder<FriendInvitation> builder)
        {
            builder.ToTable("FriendInvitations");

            builder.HasKey(fi => fi.Id);

            builder.HasIndex(fi => new { fi.SenderUserProfileId, fi.ReceiverUserProfileId, fi.Status })
                .IsUnique();

            builder.Property(fi => fi.SenderUserProfileId)
                .IsRequired();

            builder.Property(fi => fi.ReceiverUserProfileId)
                .IsRequired();

            builder.Property(fi => fi.Status)
                .IsRequired();

            builder.Property(fi => fi.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(fi => fi.RespondedAt)
                .IsRequired(false);

            builder.HasOne(fi => fi.Sender)
                .WithMany(up => up.SentFriendInvitations)
                .HasForeignKey(fi => fi.SenderUserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fi => fi.Receiver)
                .WithMany(up => up.ReceivedFriendInvitations)
                .HasForeignKey(fi => fi.ReceiverUserProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
