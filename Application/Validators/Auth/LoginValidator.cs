using Application.Common;
using Application.Common.ValidatorsExtensions;
using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("{PropertyName} must no t exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9.!_-]*$")
                .WithMessage("If you log with username, it must contain only letters, numbers, and the following special characters: . ! _ -")
                .Unless(x => Checkers.IsEmail(x.Login));

            RuleFor(x => x.Password)
                .Password();
        }
    }
}
