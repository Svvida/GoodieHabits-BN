using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class SupplementConfiguration : IEntityTypeConfiguration<Supplement>
    {
        public void Configure(EntityTypeBuilder<Supplement> builder)
        {
            builder.ToTable("Supplements");
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.UserProfileId);

            builder.Property(s => s.Name)
                .HasMaxLength(Supplement.NameMaxLength)
                .IsRequired();

            builder.Property(s => s.Unit)
                .IsRequired();

            builder.Property(s => s.DefaultAmount)
                .HasPrecision(10, 2);

            builder.Property(s => s.Note)
                .HasMaxLength(Supplement.NoteMaxLength);

            builder.Property(s => s.Color)
                .HasMaxLength(7);

            builder.Property(s => s.Icon)
                .HasMaxLength(Supplement.IconMaxLength);

            builder.Property(s => s.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.HasOne(s => s.UserProfile)
                .WithMany(u => u.Supplements)
                .HasForeignKey(s => s.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // The supplement owns its schedule slots.
            builder.HasMany(s => s.Slots)
                .WithOne(slot => slot.Supplement)
                .HasForeignKey(slot => slot.SupplementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
