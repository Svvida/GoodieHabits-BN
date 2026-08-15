using Domain.Models;
using FluentValidation;

namespace Application.Workouts.Sessions.Commands.UpdateSession
{
    public class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
    {
        public UpdateSessionCommandValidator()
        {
            RuleFor(c => c.SessionId)
                .GreaterThan(0).WithMessage("SessionId must be greater than 0.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(WorkoutSession.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Note)
                .MaximumLength(WorkoutSession.NoteMaxLength).When(c => c.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
