using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.DeleteQuest
{
    public class DeleteQuestCommandValidator : AbstractValidator<DeleteQuestCommand>
    {
        public DeleteQuestCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Quest ID must be greater than 0.")
                .QuestMustBeOwnedByCurrentUser(unitOfWork)
                .WithMessage("Quest not found or you do not have permission to access it.");
            RuleFor(x => x.AccountId)
                .GreaterThan(0).WithMessage("Account ID must be greater than 0.");
        }
    }
}
