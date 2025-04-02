using Application.Dtos.Accounts;
using Application.Validators.Helpers;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class PatchAccountValidator : AbstractValidator<PatchAccountDto>
    {
        public PatchAccountValidator()
        {
            RuleFor(x => x.Login)
                .Length(1, 15).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9.!_-]*$").WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: . ! _ -")
                .When(x => !string.IsNullOrEmpty(x.Login));

            RuleFor(x => x.Email)
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(input => Checkers.IsEmail(input!)).WithMessage("{PropertyName} must be a valid email address")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
