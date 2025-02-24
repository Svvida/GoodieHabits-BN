namespace Application.Dtos.Quests.MonthlyQuest
{
    public class UpdateMonthlyQuestDto : BaseUpdateQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
    }
}
