using Domain.Enum;

namespace Application.Dtos.Quests.OneTimeQuest
{
    public class CreateOneTimeQuestDto : BaseCreateQuestDto
    {
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.OneTime;
    }
}