using Application.Common.ValidatorsExtensions;
using FluentValidation;

namespace Application.Accounts.Commands.WipeoutData
{
    public class WipeoutDataCommandValidator : AbstractValidator<WipeoutDataCommand>
    {
        public WipeoutDataCommandValidator()
        {
            RuleFor(x => x.Password)
                .Password();

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("The confirmation password does not match.");
        }
    }
}
