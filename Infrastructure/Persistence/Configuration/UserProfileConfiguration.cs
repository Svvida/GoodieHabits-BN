using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserLevel)
                .IsRequired();

            builder.Property(p => p.Experience)
                .IsRequired();

            builder.Property(p => p.QuestsCompleted)
                .IsRequired();

            builder.HasOne(p => p.Account)
                .WithOne(a => a.Profile)
                .HasForeignKey<UserProfile>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
