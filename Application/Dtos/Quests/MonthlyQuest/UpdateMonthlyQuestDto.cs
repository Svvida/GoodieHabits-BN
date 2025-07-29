using Domain.Enum;

namespace Application.Dtos.Quests.MonthlyQuest
{
    public class UpdateMonthlyQuestDto : BaseUpdateQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Monthly;
    }
}
