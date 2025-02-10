namespace Application.Dtos.SeasonalQuest
{
    public class UpdateSeasonalQuestDto : BaseUpdateQuestDto
    {
        public required string Season { get; set; }
    }
}
