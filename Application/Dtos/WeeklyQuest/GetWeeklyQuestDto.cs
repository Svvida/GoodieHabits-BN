namespace Application.Dtos.WeeklyQuest
{
    public class GetWeeklyQuestDto : BaseGetQuestDto
    {
        public List<string> Weekdays { get; set; } = [];
        public override string? Type { get; set; } = "Weekly";
    }
}
