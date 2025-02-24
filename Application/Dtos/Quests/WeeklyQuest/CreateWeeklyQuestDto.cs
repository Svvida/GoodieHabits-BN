namespace Application.Dtos.Quests.WeeklyQuest
{
    public class CreateWeeklyQuestDto : BaseCreateQuestDto
    {
        public List<string> Weekdays { get; set; } = [];
    }
}
