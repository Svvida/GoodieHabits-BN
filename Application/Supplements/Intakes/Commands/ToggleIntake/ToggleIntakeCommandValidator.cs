using FluentValidation;

namespace Application.Supplements.Intakes.Commands.ToggleIntake
{
    public class ToggleIntakeCommandValidator : AbstractValidator<ToggleIntakeCommand>
    {
        public ToggleIntakeCommandValidator()
        {
            RuleFor(c => c.SupplementId)
                .GreaterThan(0).WithMessage("SupplementId must be greater than 0.");

            RuleFor(c => c.SlotId)
                .GreaterThan(0).WithMessage("SlotId must be greater than 0.");

            RuleFor(c => c.Amount)
                .GreaterThan(0).When(c => c.Amount.HasValue)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.WorkoutSessionId)
                .GreaterThan(0).When(c => c.WorkoutSessionId.HasValue)
                .WithMessage("WorkoutSessionId must be greater than 0.");
        }
    }
}
