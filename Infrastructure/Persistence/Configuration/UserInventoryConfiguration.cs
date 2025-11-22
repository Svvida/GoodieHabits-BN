using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class UserInventoryConfiguration : IEntityTypeConfiguration<UserInventory>
    {
        public void Configure(EntityTypeBuilder<UserInventory> builder)
        {
            builder.ToTable("UserInventories");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserProfileId, x.ShopItemId }).IsUnique();

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.UserProfileId)
                .IsRequired();

            builder.Property(x => x.ShopItemId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(x => x.AcquiredAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(x => x.UserProfile)
                .WithMany(x => x.InventoryItems)
                .HasForeignKey(x => x.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ShopItem)
                .WithMany(x => x.UserInventories)
                .HasForeignKey(x => x.ShopItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
