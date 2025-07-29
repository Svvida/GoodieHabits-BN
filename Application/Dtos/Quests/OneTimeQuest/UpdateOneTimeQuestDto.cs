using Domain.Enum;

namespace Application.Dtos.Quests.OneTimeQuest
{
    public class UpdateOneTimeQuestDto : BaseUpdateQuestDto
    {
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.OneTime;
    }
}
