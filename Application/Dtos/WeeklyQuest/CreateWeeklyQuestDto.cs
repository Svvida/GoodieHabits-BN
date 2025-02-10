namespace Application.Dtos.WeeklyQuest
{
    public class CreateWeeklyQuestDto : BaseCreateQuestDto
    {
        public List<string> Weekdays { get; set; } = [];
    }
}
