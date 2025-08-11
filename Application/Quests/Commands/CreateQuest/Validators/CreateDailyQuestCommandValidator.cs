using Application.Quests.Commands.CreateQuest;
using Domain.Interfaces;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateDailyQuestCommandValidator(IUnitOfWork unitOfWork) : CreateQuestCommandValidator<CreateDailyQuestCommand>(unitOfWork)
    {
    }
}
