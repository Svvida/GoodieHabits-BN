using Domain.Enum;

namespace Application.Dtos.Quests.WeeklyQuest
{
    public class CreateWeeklyQuestDto : BaseCreateQuestDto
    {
        public List<string> Weekdays { get; set; } = [];
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Weekly;
    }
}
