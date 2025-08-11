using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.QuestLabels.Commands.UpdateQuestLabel
{
    public class UpdateQuestLabelCommandValidator : AbstractValidator<UpdateQuestLabelCommand>
    {

        public UpdateQuestLabelCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(cmd => cmd.LabelId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Label ID must be greater than 0.")
                .QuestLabelMustBeOwnedByCurrentUser(unitOfWork)
                .WithMessage("Label not found or you do not have permission to access it.");

            RuleFor(cmd => cmd.Value)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Value is required")
                .MaximumLength(25).WithMessage("Value must not exceed {MaxLength} characters")
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