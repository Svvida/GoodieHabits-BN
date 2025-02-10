namespace Application.Dtos.WeeklyQuest
{
    public class UpdateWeeklyQuestDto : BaseUpdateQuestDto
    {
        public List<string> Weekdays { get; set; } = null!;
    }
}
