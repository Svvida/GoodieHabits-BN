using Domain.Models;
using FluentValidation;

namespace Application.Workouts.Sessions.Commands.StartSession
{
    public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
    {
        public StartSessionCommandValidator()
        {
            RuleFor(c => c.RoutineId)
                .GreaterThan(0).When(c => c.RoutineId.HasValue)
                .WithMessage("RoutineId must be greater than 0.");

            // An ad-hoc session has no routine to borrow a name from, so it must bring its own.
            RuleFor(c => c.Name)
                .NotEmpty().When(c => c.RoutineId is null)
                .WithMessage("Name is required when starting a session without a routine.");

            RuleFor(c => c.Name)
                .MaximumLength(WorkoutSession.NameMaxLength).When(c => c.Name is not null)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Note)
                .MaximumLength(WorkoutSession.NoteMaxLength).When(c => c.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
