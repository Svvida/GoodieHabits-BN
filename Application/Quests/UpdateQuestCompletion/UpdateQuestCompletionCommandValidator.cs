using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.UpdateQuestCompletion
{
    public class UpdateQuestCompletionCommandValidator : AbstractValidator<UpdateQuestCompletionCommand>
    {
        public UpdateQuestCompletionCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("QuestId must be greater than 0.")
                .QuestMustBeOwnedByCurrentUser(unitOfWork)
                .WithMessage("Quest not found or you do not have permission to access it.");
        }
    }
}
