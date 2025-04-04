using Application.Dtos.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -");

            RuleFor(x => x.ConfirmOldPassword)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -")
                .Equal(x => x.OldPassword)
                .WithMessage("The confirmation password does not match your old password.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -")
                .NotEqual(x => x.OldPassword)
                .WithMessage("New password must be different from the old password.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -")
                .Equal(x => x.NewPassword)
                .WithMessage("The confirmation password does not match your new password.");
        }
    }
}
