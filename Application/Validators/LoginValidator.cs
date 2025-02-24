using System.Text.RegularExpressions;
using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators
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
                .Unless(x => IsEmail(x.Login));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -");
        }

        private static bool IsEmail(string login) =>
            Regex.IsMatch(login, @"^(?!.*\.\.)[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    }
}
