namespace Application.Dtos.Quests.WeeklyQuest
{
    public class PatchWeeklyQuestDto : BasePatchQuestDto
    {
        public List<string>? Weekdays { get; set; }
    }
}
