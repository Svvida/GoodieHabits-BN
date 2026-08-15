using Application.Workouts.Sessions.Common;
using FluentValidation;

namespace Application.Workouts.Sessions.Commands.LogSession
{
    public class LogSessionCommandValidator : AbstractValidator<LogSessionCommand>
    {
        public LogSessionCommandValidator()
        {
            RuleFor(c => c.SessionId)
                .GreaterThan(0).WithMessage("SessionId must be greater than 0.");

            RuleFor(c => c.Exercises)
                .NotNull().WithMessage("Exercises must not be null (send an empty array to clear the log).");

            RuleForEach(c => c.Exercises).SetValidator(new SessionExerciseInputValidator());
        }
    }
}
