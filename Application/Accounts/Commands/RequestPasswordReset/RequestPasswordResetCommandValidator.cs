using Application.Accounts.Commands.RequestPasswordReset;
using Application.Common;
using FluentValidation;

namespace Application.Accounts.Commands.ForgotPassword
{
    public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(Checkers.IsEmail).WithMessage("Invalid {PropertyName} format.");
        }
    }
}
