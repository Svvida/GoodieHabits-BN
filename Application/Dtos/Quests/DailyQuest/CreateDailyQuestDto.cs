using Domain.Enum;

namespace Application.Dtos.Quests.DailyQuest
{
    public class CreateDailyQuestDto : BaseCreateQuestDto
    {
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Daily;
    }
}
