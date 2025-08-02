using Application.Common.ValidatorsExtensions;
using FluentValidation;

namespace Application.Accounts.Commands.DeleteAccount
{
    public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountCommandValidator()
        {
            RuleFor(x => x.Password)
                .Password();

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("The confirmation password does not match.");
        }
    }
}
