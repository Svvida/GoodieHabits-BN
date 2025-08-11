using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.ValidatorsExtensions;
using FluentValidation;

namespace Application.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Custom((login, context) =>
                {
                    if (Checkers.IsEmail(login))
                    {
                    }
                    else
                    {
                        if (!Regex.IsMatch(login, "^[a-zA-Z0-9.!_-]*$"))
                            context.AddFailure("Login", "Username must contain only letters, numbers, and the following special characters: . ! _ -");
                    }
                });

            RuleFor(x => x.Password).Password();
        }
    }
}
