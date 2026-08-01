using Application.Quests.Commands.CreateQuest;
using Domain.Interfaces;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateOneTimeQuestCommandValidator(IUnitOfWork unitOfWork, IClock clock) : CreateQuestCommandValidator<CreateOneTimeQuestCommand>(unitOfWork, clock)
    {
    }
}
