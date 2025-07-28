using Application.Dtos.Quests.OneTimeQuest;
using Domain.Interfaces;

namespace Application.Validators.Quests.OneTimeQuest
{
    public class CreateOneTimeQuestValidator : BaseCreateQuestValidator<CreateOneTimeQuestDto>
    {
        public CreateOneTimeQuestValidator(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
}
