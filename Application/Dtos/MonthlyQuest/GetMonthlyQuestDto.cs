namespace Application.Dtos.MonthlyQuest
{
    public class GetMonthlyQuestDto : BaseGetQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public override string? Type { get; set; } = "Monthly";
    }
}
