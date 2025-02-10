namespace Application.Dtos.MonthlyQuest
{
    public class UpdateMonthlyQuestDto : BaseUpdateQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
    }
}
