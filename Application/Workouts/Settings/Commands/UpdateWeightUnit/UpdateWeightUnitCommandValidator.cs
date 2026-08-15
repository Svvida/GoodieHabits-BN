using Domain.ValueObjects;
using FluentValidation;

namespace Application.Workouts.Settings.Commands.UpdateWeightUnit
{
    public class UpdateWeightUnitCommandValidator : AbstractValidator<UpdateWeightUnitCommand>
    {
        public UpdateWeightUnitCommandValidator()
        {
            RuleFor(c => c.WeightUnit)
                .NotEmpty().WithMessage("WeightUnit is required.")
                .Must(SupportedWeightUnits.IsSupported)
                .WithMessage($"WeightUnit must be one of: {string.Join(", ", SupportedWeightUnits.All)}.");
        }
    }
}
