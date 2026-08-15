using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class SupplementScheduleSlotConfiguration : IEntityTypeConfiguration<SupplementScheduleSlot>
    {
        public void Configure(EntityTypeBuilder<SupplementScheduleSlot> builder)
        {
            builder.ToTable("SupplementScheduleSlots");
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.SupplementId);

            builder.Property(s => s.Timing)
                .IsRequired();

            // TimeOnly maps to SQL `time`. A local clock time, not an instant — same reasoning as the
            // scheduledTime on quests.
            builder.Property(s => s.TimeOfDay)
                .HasColumnType("time");

            builder.Property(s => s.Amount)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(s => s.Note)
                .HasMaxLength(SupplementScheduleSlot.NoteMaxLength);
        }
    }
}
