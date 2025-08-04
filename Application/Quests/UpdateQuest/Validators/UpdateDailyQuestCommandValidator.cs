using Domain.Interfaces;

namespace Application.Quests.UpdateQuest.Validators
{
    public class UpdateDailyQuestCommandValidator(IUnitOfWork unitOfWork) : UpdateQuestCommandValidator<UpdateDailyQuestCommand>(unitOfWork)
    {
    }
}
