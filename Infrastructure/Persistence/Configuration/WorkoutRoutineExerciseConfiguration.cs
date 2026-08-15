using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WorkoutRoutineExerciseConfiguration : IEntityTypeConfiguration<WorkoutRoutineExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutRoutineExercise> builder)
        {
            builder.ToTable("WorkoutRoutineExercises");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.WorkoutRoutineId, e.Order });
            builder.HasIndex(e => e.ExerciseId);

            builder.Property(e => e.Order)
                .IsRequired();

            builder.Property(e => e.TargetWeight)
                .HasPrecision(6, 2);

            builder.Property(e => e.TargetDistance)
                .HasPrecision(8, 2);

            builder.Property(e => e.Note)
                .HasMaxLength(WorkoutRoutineExercise.NoteMaxLength);

            // Restrict: deleting an exercise still planned in a routine is blocked, and the handler names the
            // offending routines rather than letting the database silently rewrite someone's plan.
            builder.HasOne(e => e.Exercise)
                .WithMany(x => x.RoutineExercises)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
