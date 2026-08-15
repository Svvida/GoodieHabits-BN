using Application.Workouts.Sessions.Dtos;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Workouts.Sessions.Common
{
    /// <summary>
    /// Field bounds for a logged set, from the same <see cref="WorkoutLimits"/> constants the entity guards use.
    /// <para>
    /// Note what is deliberately <b>not</b> here: whether a set carries the measurements its exercise's metric
    /// requires. That needs the entry's snapshotted metric, which only the handler has, so the entity enforces
    /// it and surfaces a 400 of its own. Restating the metric table here would create two versions of it.
    /// </para>
    /// </summary>
    public class SessionSetInputValidator : AbstractValidator<SessionSetInput>
    {
        public SessionSetInputValidator()
        {
            RuleFor(s => s.Reps)
                .InclusiveBetween(0, WorkoutLimits.MaxReps).When(s => s.Reps.HasValue)
                .WithMessage($"Reps must be between 0 and {WorkoutLimits.MaxReps}.");

            RuleFor(s => s.Weight)
                .InclusiveBetween(0m, WorkoutLimits.MaxWeight).When(s => s.Weight.HasValue)
                .WithMessage($"Weight must be between 0 and {WorkoutLimits.MaxWeight}.");

            RuleFor(s => s.DurationSeconds)
                .InclusiveBetween(0, WorkoutLimits.MaxDurationSeconds).When(s => s.DurationSeconds.HasValue)
                .WithMessage($"DurationSeconds must be between 0 and {WorkoutLimits.MaxDurationSeconds}.");

            RuleFor(s => s.Distance)
                .InclusiveBetween(0m, WorkoutLimits.MaxDistance).When(s => s.Distance.HasValue)
                .WithMessage($"Distance must be between 0 and {WorkoutLimits.MaxDistance}.");

            RuleFor(s => s.Rpe)
                .InclusiveBetween(WorkoutLimits.MinRpe, WorkoutLimits.MaxRpe).When(s => s.Rpe.HasValue)
                .WithMessage($"Rpe must be between {WorkoutLimits.MinRpe} and {WorkoutLimits.MaxRpe}.");

            RuleFor(s => s.SetType)
                .IsInEnum().WithMessage("SetType is invalid.");
        }
    }
}
