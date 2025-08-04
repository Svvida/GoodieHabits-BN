using Domain.Interfaces;
using FluentValidation;

namespace Application.QuestLabels.CreateQuestLabel
{
    public class CreateQuestLabelCommandValidator : AbstractValidator<CreateQuestLabelCommand>
    {
        public CreateQuestLabelCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(cmd => cmd.Value)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(25).WithMessage("{PropertyName} must not exceed {MaxLength} characters")
                .MustAsync(async (command, value, cancellationToken) =>
                {
                    return await unitOfWork.QuestLabels.IsLabelValueUniqueForUser(value, command.AccountId, cancellationToken);
                })
                .WithMessage("A label with value '{PropertyValue}' already exists for this account.");

            RuleFor(cmd => cmd.BackgroundColor)
                .NotEmpty().WithMessage("Background Color is required")
                .MaximumLength(7).WithMessage("Background Color must not exceed {MaxLength} characters");
        }
    }
}

