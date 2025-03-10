namespace Application.Dtos.Quests.OneTimeQuest
{
    public class GetOneTimeQuestDto : BaseGetQuestDto
    {
        public override string? Type { get; set; } = "OneTime";
    }
}
