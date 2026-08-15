using Application.Workouts.Sessions.Common;
using FluentValidation;

namespace Application.Workouts.Sessions.Commands.AddSessionExercise
{
    public class AddSessionExerciseCommandValidator : AbstractValidator<AddSessionExerciseCommand>
    {
        public AddSessionExerciseCommandValidator()
        {
            RuleFor(c => c.SessionId)
                .GreaterThan(0).WithMessage("SessionId must be greater than 0.");

            RuleFor(c => c.Exercise)
                .NotNull().WithMessage("Exercise is required.");

            RuleFor(c => c.Exercise)
                .SetValidator(new SessionExerciseInputValidator()!)
                .When(c => c.Exercise is not null);
        }
    }
}
