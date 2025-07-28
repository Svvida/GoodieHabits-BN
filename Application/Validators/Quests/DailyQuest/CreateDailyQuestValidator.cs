using Application.Dtos.Quests.DailyQuest;
using Domain.Interfaces;

namespace Application.Validators.Quests.DailyQuest
{
    public class CreateDailyQuestValidator : BaseCreateQuestValidator<CreateDailyQuestDto>
    {
        public CreateDailyQuestValidator(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
}
