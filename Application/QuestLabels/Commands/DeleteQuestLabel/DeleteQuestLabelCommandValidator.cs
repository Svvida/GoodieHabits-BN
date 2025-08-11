using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public class DeleteQuestLabelCommandValidator : AbstractValidator<DeleteQuestLabelCommand>
    {
        public DeleteQuestLabelCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(cmd => cmd.Id)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Label ID must be greater than 0.")
                .QuestLabelMustBeOwnedByCurrentUser(unitOfWork)
                .WithMessage("Label not found or you do not have permission to access it.");
        }
    }
}