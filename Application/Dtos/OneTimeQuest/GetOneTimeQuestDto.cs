namespace Application.Dtos.OneTimeQuest
{
    public class GetOneTimeQuestDto : BaseGetQuestDto
    {
        public override string? Type { get; set; } = "One-Time";
    }
}
