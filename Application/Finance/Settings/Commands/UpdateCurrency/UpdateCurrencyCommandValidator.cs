using Domain.ValueObjects;
using FluentValidation;

namespace Application.Finance.Settings.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
    {
        public UpdateCurrencyCommandValidator()
        {
            RuleFor(c => c.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Must(SupportedCurrencies.IsSupported)
                .WithMessage("Currency is not supported.");
        }
    }
}
