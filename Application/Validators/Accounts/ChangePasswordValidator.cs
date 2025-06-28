using Application.Dtos.Accounts;
using Application.ValidatorsExtensions;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword)
                .Password();

            RuleFor(x => x.NewPassword)
                .Password()
                .NotEqual(x => x.OldPassword)
                    .WithMessage("New password must be different from the old password.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword)
                    .WithMessage("The confirmation password does not match your new password.");
        }
    }
}
