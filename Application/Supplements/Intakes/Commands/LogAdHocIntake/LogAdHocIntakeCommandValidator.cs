using FluentValidation;

namespace Application.Supplements.Intakes.Commands.LogAdHocIntake
{
    public class LogAdHocIntakeCommandValidator : AbstractValidator<LogAdHocIntakeCommand>
    {
        public LogAdHocIntakeCommandValidator()
        {
            RuleFor(c => c.SupplementId)
                .GreaterThan(0).WithMessage("SupplementId must be greater than 0.");

            RuleFor(c => c.Amount)
                .GreaterThan(0).When(c => c.Amount.HasValue)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.WorkoutSessionId)
                .GreaterThan(0).When(c => c.WorkoutSessionId.HasValue)
                .WithMessage("WorkoutSessionId must be greater than 0.");
        }
    }
}
