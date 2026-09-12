using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Queries.GetQuestById
{
    public class GetQuestByIdQueryValidator : AbstractValidator<GetQuestByIdQuery>
    {
        public GetQuestByIdQueryValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId).QuestMustBeOwnedByCurrentUser(unitOfWork);
        }
    }
}
