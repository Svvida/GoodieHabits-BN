namespace Application.Dtos.SeasonalQuest
{
    public class CreateSeasonalQuestDto : BaseCreateQuestDto
    {
        public required string Season { get; set; }
    }
}
