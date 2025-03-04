using Application.Dtos.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class PatchAccountValidator : AbstractValidator<PatchAccountDto>
    {
        public PatchAccountValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Length(1, 20).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9.!_-]*$").WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: . ! _ -");
        }
    }
}
