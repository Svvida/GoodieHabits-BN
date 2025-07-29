using Domain.Enum;

namespace Application.Dtos.Quests.WeeklyQuest
{
    public class UpdateWeeklyQuestDto : BaseUpdateQuestDto
    {
        public List<string> Weekdays { get; set; } = null!;
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Weekly;
    }
}
