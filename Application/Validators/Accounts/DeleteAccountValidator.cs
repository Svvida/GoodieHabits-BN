using Application.Dtos.Accounts;
using Application.ValidatorsExtensions;
using FluentValidation;

namespace Application.Validators.Accounts
{
    public class DeleteAccountValidator : AbstractValidator<DeleteAccountDto>
    {
        public DeleteAccountValidator()
        {
            RuleFor(x => x.Password)
                .Password();
        }
    }
}
