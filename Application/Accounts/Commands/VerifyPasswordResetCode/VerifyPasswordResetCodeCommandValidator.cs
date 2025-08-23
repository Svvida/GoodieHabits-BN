using FluentValidation;

namespace Application.Accounts.Commands.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeCommandValidator : AbstractValidator<VerifyPasswordResetCodeCommand>
    {
        public VerifyPasswordResetCodeCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
                .Must(Common.Checkers.IsEmail).WithMessage("Invalid {PropertyName} format.");
            RuleFor(x => x.ResetCode)
                .NotEmpty().WithMessage("Reset code is required.")
                .Length(6).WithMessage("Reset code must be exactly 6 digits.")
                .Matches("^[0-9]{6}$").WithMessage("Reset code must only contain digits.");
        }
    }
}
