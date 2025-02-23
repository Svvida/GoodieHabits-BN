using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 8 characters long")
                .MaximumLength(50).WithMessage("Password must not exceed 50 characters")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("Password must contain only letters, numbers, and the following special characters: _ @ # -");
        }
    }
}
