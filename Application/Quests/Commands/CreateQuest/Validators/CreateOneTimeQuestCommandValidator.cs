using Application.Quests.Commands.CreateQuest;
using Domain.Interfaces;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateOneTimeQuestCommandValidator(IUnitOfWork unitOfWork) : CreateQuestCommandValidator<CreateOneTimeQuestCommand>(unitOfWork)
    {
    }
}
