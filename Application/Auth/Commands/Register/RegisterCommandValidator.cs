using Application.Common;
using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IUnitOfWork unitOfWork)
        {

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(Checkers.IsEmail).WithMessage("Invalid {PropertyName} format.")
                .MustAsync(unitOfWork.Accounts.IsEmailUniqueAsync)
                .WithMessage("This email address is already in use.");
            RuleFor(x => x.Password).Password();
        }
    }
}
