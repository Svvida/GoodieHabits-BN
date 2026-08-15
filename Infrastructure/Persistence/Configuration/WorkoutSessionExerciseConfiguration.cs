using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WorkoutSessionExerciseConfiguration : IEntityTypeConfiguration<WorkoutSessionExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutSessionExercise> builder)
        {
            builder.ToTable("WorkoutSessionExercises");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.WorkoutSessionId, e.Order });

            // Analytics read path: "every time I did this exercise".
            builder.HasIndex(e => e.ExerciseId);

            // Snapshots. ExerciseName and MetricType are what history renders from, so they are required even
            // though the library row they came from may since have been renamed, re-typed or deleted.
            builder.Property(e => e.ExerciseName)
                .HasMaxLength(Exercise.NameMaxLength)
                .IsRequired();

            builder.Property(e => e.MetricType)
                .IsRequired();

            builder.Property(e => e.Order)
                .IsRequired();

            builder.Property(e => e.TargetWeight)
                .HasPrecision(6, 2);

            builder.Property(e => e.TargetDistance)
                .HasPrecision(8, 2);

            builder.Property(e => e.Note)
                .HasMaxLength(WorkoutSessionExercise.NoteMaxLength);

            // Restrict rather than SetNull, with the exercise delete handler clearing the FK explicitly: the
            // codebase's established shape for "this row outlives what it points at" (see RecurringTransaction).
            builder.HasOne(e => e.Exercise)
                .WithMany(x => x.SessionExercises)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Sets)
                .WithOne(s => s.WorkoutSessionExercise)
                .HasForeignKey(s => s.WorkoutSessionExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
