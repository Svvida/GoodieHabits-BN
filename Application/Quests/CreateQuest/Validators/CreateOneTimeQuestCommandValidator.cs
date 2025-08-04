using Domain.Interfaces;

namespace Application.Quests.CreateQuest.Validators
{
    public class CreateOneTimeQuestCommandValidator(IUnitOfWork unitOfWork) : CreateQuestCommandValidator<CreateOneTimeQuestCommand>(unitOfWork)
    {
    }
}
