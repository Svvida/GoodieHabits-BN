using Application.Dtos.Accounts;
using Application.Validators.Helpers;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class UpdateAccountValidator : AbstractValidator<UpdateAccountDto>
    {
        public UpdateAccountValidator()
        {
            RuleFor(x => x.Login)
                .Length(3, 15).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9.!_-]*$").WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: . ! _ -")
                .When(x => !string.IsNullOrEmpty(x.Login));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(input => Checkers.IsEmail(input)).WithMessage("{PropertyName} must be a valid email address");

            RuleFor(x => x.Nickname)
                .Length(3, 15).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .When(x => !string.IsNullOrEmpty(x.Nickname));

            RuleFor(x => x.Bio)
                .MaximumLength(150).WithMessage("{PropertyName} cannot exceed {MaxLength} characters")
                .Must(input => Checkers.IsSafeHtml(input)).WithMessage("{PropertyName} contains unsafe HTML")
                .When(x => !string.IsNullOrEmpty(x.Bio));
        }
    }
}
