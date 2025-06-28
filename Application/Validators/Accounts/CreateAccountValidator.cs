using Application.Dtos.Accounts;
using Application.Validators.Helpers;
using Application.ValidatorsExtensions;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.Password)
                .Password();

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
                .Must(input => Checkers.IsEmail(input!)).WithMessage("Email must be a valid email address");
        }
    }
}
