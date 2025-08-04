using Domain.Interfaces;

namespace Application.Quests.UpdateQuest.Validators
{
    public class UpdateOneTimeQuestCommandValidator(IUnitOfWork unitOfWork) : UpdateQuestCommandValidator<UpdateOneTimeQuestCommand>(unitOfWork)
    {
    }
}
