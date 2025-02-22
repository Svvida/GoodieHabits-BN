namespace Application.Dtos.Quests.DailyQuest
{
    public class GetDailyQuestDto : BaseGetQuestDto
    {
        public override string? Type { get; set; } = "Daily";
    }
}
