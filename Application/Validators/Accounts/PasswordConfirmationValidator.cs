using Application.Dtos.Auth;
using Application.ValidatorsExtensions;
using FluentValidation;

namespace Application.Validators.Accounts
{
    internal class PasswordConfirmationValidator : AbstractValidator<PasswordConfirmationDto>
    {
        public PasswordConfirmationValidator()
        {
            RuleFor(x => x.Password)
                .Password();

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("The confirmation password does not match.");
        }
    }
}
