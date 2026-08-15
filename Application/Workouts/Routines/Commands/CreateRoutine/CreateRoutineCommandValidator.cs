using Application.Workouts.Routines.Common;
using Domain.Models;
using FluentValidation;

namespace Application.Workouts.Routines.Commands.CreateRoutine
{
    public class CreateRoutineCommandValidator : AbstractValidator<CreateRoutineCommand>
    {
        public CreateRoutineCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(WorkoutRoutine.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Description)
                .MaximumLength(WorkoutRoutine.DescriptionMaxLength).When(c => c.Description is not null)
                .WithMessage("Description must not exceed {MaxLength} characters.");

            RuleFor(c => c.Exercises)
                .NotNull().WithMessage("Exercises must not be null (send an empty array instead).");

            RuleForEach(c => c.Exercises).SetValidator(new RoutineExerciseInputValidator());
        }
    }
}
