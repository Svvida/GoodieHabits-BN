using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WorkoutRoutineConfiguration : IEntityTypeConfiguration<WorkoutRoutine>
    {
        public void Configure(EntityTypeBuilder<WorkoutRoutine> builder)
        {
            builder.ToTable("WorkoutRoutines");
            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.UserProfileId);

            builder.Property(r => r.Name)
                .HasMaxLength(WorkoutRoutine.NameMaxLength)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasMaxLength(WorkoutRoutine.DescriptionMaxLength);

            builder.Property(r => r.IsArchived)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(r => r.UserProfile)
                .WithMany(u => u.WorkoutRoutines)
                .HasForeignKey(r => r.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // The routine owns its items — replacing the list deletes the removed ones.
            builder.HasMany(r => r.Exercises)
                .WithOne(e => e.WorkoutRoutine)
                .HasForeignKey(e => e.WorkoutRoutineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
