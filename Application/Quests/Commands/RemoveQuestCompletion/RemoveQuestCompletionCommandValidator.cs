using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.RemoveQuestCompletion
{
    public class RemoveQuestCompletionCommandValidator : AbstractValidator<RemoveQuestCompletionCommand>
    {
        public RemoveQuestCompletionCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId).QuestMustBeOwnedByCurrentUser(unitOfWork);
        }
    }
}
