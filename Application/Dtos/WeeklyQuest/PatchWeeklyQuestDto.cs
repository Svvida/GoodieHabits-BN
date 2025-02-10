namespace Application.Dtos.WeeklyQuest
{
    public class PatchWeeklyQuestDto : BasePatchQuestDto
    {
        public List<string>? Weekdays { get; set; }
    }
}
