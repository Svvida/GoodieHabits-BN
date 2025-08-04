using Domain.Interfaces;

namespace Application.Quests.CreateQuest.Validators
{
    public class CreateDailyQuestCommandValidator(IUnitOfWork unitOfWork) : CreateQuestCommandValidator<CreateDailyQuestCommand>(unitOfWork)
    {
    }
}
