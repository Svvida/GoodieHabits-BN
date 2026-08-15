using Domain.Models;
using FluentValidation;

namespace Application.Workouts.Exercises.Commands.UpdateExercise
{
    public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
    {
        public UpdateExerciseCommandValidator()
        {
            RuleFor(c => c.ExerciseId)
                .GreaterThan(0).WithMessage("ExerciseId must be greater than 0.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(Exercise.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.MetricType)
                .IsInEnum().WithMessage("MetricType is invalid.");

            RuleFor(c => c.MuscleGroup)
                .IsInEnum().WithMessage("MuscleGroup is invalid.");

            RuleFor(c => c.Equipment)
                .IsInEnum().WithMessage("Equipment is invalid.");

            RuleFor(c => c.Note)
                .MaximumLength(Exercise.NoteMaxLength).When(c => c.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
