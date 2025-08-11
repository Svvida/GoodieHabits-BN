using Application.Quests.Commands.UpdateQuest;
using Domain.Interfaces;

namespace Application.Quests.Commands.UpdateQuest.Validators
{
    public class UpdateDailyQuestCommandValidator(IUnitOfWork unitOfWork) : UpdateQuestCommandValidator<UpdateDailyQuestCommand>(unitOfWork)
    {
    }
}
