using Application.Common;
using FluentValidation;

namespace Application.Accounts.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(Checkers.IsEmail).WithMessage("Invalid {PropertyName} format.");
        }
    }
}
