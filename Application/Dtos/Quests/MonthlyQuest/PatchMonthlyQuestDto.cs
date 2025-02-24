namespace Application.Dtos.Quests.MonthlyQuest
{
    public class PatchMonthlyQuestDto : BasePatchQuestDto
    {
        public int? StartDay { get; set; }
        public int? EndDay { get; set; }
    }
}
