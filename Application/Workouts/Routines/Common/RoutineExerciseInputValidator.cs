using Application.Workouts.Routines.Dtos;
using Domain.Models;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Workouts.Routines.Common
{
    /// <summary>
    /// Field rules for one planned exercise. The bounds come from <see cref="WorkoutLimits"/>, the same
    /// constants the entity guards use — so a validator and an entity can never disagree about what is legal.
    /// </summary>
    public class RoutineExerciseInputValidator : AbstractValidator<RoutineExerciseInput>
    {
        public RoutineExerciseInputValidator()
        {
            RuleFor(i => i.ExerciseId)
                .GreaterThan(0).WithMessage("ExerciseId must be greater than 0.");

            RuleFor(i => i.TargetSets)
                .InclusiveBetween(1, WorkoutLimits.MaxSets).When(i => i.TargetSets.HasValue)
                .WithMessage($"TargetSets must be between 1 and {WorkoutLimits.MaxSets}.");

            RuleFor(i => i.TargetReps)
                .InclusiveBetween(1, WorkoutLimits.MaxReps).When(i => i.TargetReps.HasValue)
                .WithMessage($"TargetReps must be between 1 and {WorkoutLimits.MaxReps}.");

            RuleFor(i => i.TargetWeight)
                .InclusiveBetween(0m, WorkoutLimits.MaxWeight).When(i => i.TargetWeight.HasValue)
                .WithMessage($"TargetWeight must be between 0 and {WorkoutLimits.MaxWeight}.");

            RuleFor(i => i.TargetDurationSeconds)
                .InclusiveBetween(1, WorkoutLimits.MaxDurationSeconds).When(i => i.TargetDurationSeconds.HasValue)
                .WithMessage($"TargetDurationSeconds must be between 1 and {WorkoutLimits.MaxDurationSeconds}.");

            RuleFor(i => i.TargetDistance)
                .InclusiveBetween(0m, WorkoutLimits.MaxDistance).When(i => i.TargetDistance.HasValue)
                .WithMessage($"TargetDistance must be between 0 and {WorkoutLimits.MaxDistance}.");

            RuleFor(i => i.RestSeconds)
                .InclusiveBetween(0, WorkoutLimits.MaxRestSeconds).When(i => i.RestSeconds.HasValue)
                .WithMessage($"RestSeconds must be between 0 and {WorkoutLimits.MaxRestSeconds}.");

            RuleFor(i => i.Note)
                .MaximumLength(WorkoutRoutineExercise.NoteMaxLength).When(i => i.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
