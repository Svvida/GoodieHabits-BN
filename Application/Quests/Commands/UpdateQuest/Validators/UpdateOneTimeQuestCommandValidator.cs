using Application.Quests.Commands.UpdateQuest;
using Domain.Interfaces;

namespace Application.Quests.Commands.UpdateQuest.Validators
{
    public class UpdateOneTimeQuestCommandValidator(IUnitOfWork unitOfWork) : UpdateQuestCommandValidator<UpdateOneTimeQuestCommand>(unitOfWork)
    {
    }
}
