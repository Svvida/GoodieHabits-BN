using Domain.Enum;

namespace Application.Dtos.Quests.DailyQuest
{
    public class UpdateDailyQuestDto : BaseUpdateQuestDto
    {
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Daily;
    }
}
