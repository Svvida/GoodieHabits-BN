using Application.Common;
using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Auth.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public RegisterCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(Checkers.IsEmail).WithMessage("Invalid {PropertyName} format.")
                .MustAsync(_unitOfWork.Accounts.IsEmailUniqueAsync)
                .WithMessage("This email address is already in use.");
            RuleFor(x => x.Password).Password();
        }
    }
}
