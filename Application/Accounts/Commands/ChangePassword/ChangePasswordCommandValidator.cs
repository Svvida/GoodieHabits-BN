using Application.Common.ValidatorsExtensions;
using FluentValidation;

namespace Application.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
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
