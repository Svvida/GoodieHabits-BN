using Application.Workouts.Routines.Common;
using Domain.Models;
using FluentValidation;

namespace Application.Workouts.Routines.Commands.UpdateRoutine
{
    public class UpdateRoutineCommandValidator : AbstractValidator<UpdateRoutineCommand>
    {
        public UpdateRoutineCommandValidator()
        {
            RuleFor(c => c.RoutineId)
                .GreaterThan(0).WithMessage("RoutineId must be greater than 0.");

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
