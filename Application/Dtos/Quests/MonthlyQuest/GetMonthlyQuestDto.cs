namespace Application.Dtos.Quests.MonthlyQuest
{
    public class GetMonthlyQuestDto : BaseGetQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
    }
}
