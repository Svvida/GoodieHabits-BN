using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
    {
        public void Configure(EntityTypeBuilder<WorkoutSet> builder)
        {
            builder.ToTable("WorkoutSets");
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => new { s.WorkoutSessionExerciseId, s.SetNumber });

            builder.Ignore(s => s.IsWarmup);

            builder.Property(s => s.SetNumber)
                .IsRequired();

            // Every measurement is nullable; which ones are required is decided by the parent entry's
            // snapshotted metric, not by the schema.
            builder.Property(s => s.Weight)
                .HasPrecision(6, 2);

            builder.Property(s => s.Distance)
                .HasPrecision(8, 2);

            builder.Property(s => s.Rpe)
                .HasPrecision(3, 1);

            builder.Property(s => s.SetType)
                .IsRequired();

            builder.Property(s => s.CompletedAt)
                .IsRequired();
        }
    }
}
