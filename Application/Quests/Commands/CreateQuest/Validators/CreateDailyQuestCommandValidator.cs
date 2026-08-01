using Application.Quests.Commands.CreateQuest;
using Domain.Interfaces;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateDailyQuestCommandValidator(IUnitOfWork unitOfWork, IClock clock) : CreateQuestCommandValidator<CreateDailyQuestCommand>(unitOfWork, clock)
    {
    }
}
