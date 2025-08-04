using Application.Common;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Accounts.UpdateAccount
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAccountCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.AccountId).NotEmpty();

            RuleFor(x => x.Login)
                .Length(3, 16).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9.!_-]*$").WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: . ! _ -")
                .MustAsync(BeUniqueLoginAsync).WithMessage("This {PropertyName} is already taken.")
                .When(x => !string.IsNullOrWhiteSpace(x.Login));


            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters")
                .Must(Checkers.IsEmail).WithMessage("{PropertyName} must be a valid {PropertyName} address")
                .MustAsync(BeUniqueEmailAsync).WithMessage("This {PropertyName} address is already in use.");

            RuleFor(x => x.Nickname)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Length(3, 30).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .MustAsync(BeUniqueNicknameAsync).WithMessage("This {PropertyName} is already taken.");

            RuleFor(x => x.Bio)
                .MaximumLength(30).WithMessage("{PropertyName} cannot exceed {MaxLength} characters")
                .Must(Checkers.IsSafeHtml).WithMessage("{PropertyName} contains unsafe HTML")
                .When(x => !string.IsNullOrEmpty(x.Bio));
        }

        private async Task<bool> BeUniqueLoginAsync(UpdateAccountCommand command, string login, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.Accounts.DoesLoginExistAsync(login, command.AccountId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> BeUniqueEmailAsync(UpdateAccountCommand command, string email, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.Accounts.DoesEmailExistAsync(email, command.AccountId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> BeUniqueNicknameAsync(UpdateAccountCommand command, string nickname, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.UserProfiles.DoesNicknameExistAsync(nickname, command.AccountId, cancellationToken).ConfigureAwait(false);
        }
    }
}