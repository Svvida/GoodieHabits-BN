namespace Application.Dtos.SeasonalQuest
{
    public class GetSeasonalQuestDto : BaseGetQuestDto
    {
        public required string Season { get; set; }
        public override string? Type { get; set; } = "Seasonal";
    }
}
