using Application.Dtos.Accounts;
using Application.Validators.Helpers;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .MaximumLength(50).WithMessage("Password must not exceed 50 characters")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("Password must contain only letters, numbers, and the following special characters: _ @ # -");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
                .Must(input => Checkers.IsEmail(input!)).WithMessage("Email must be a valid email address");
        }
    }
}
