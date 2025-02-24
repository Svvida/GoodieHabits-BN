namespace Application.Dtos.Quests.SeasonalQuest
{
    public class UpdateSeasonalQuestDto : BaseUpdateQuestDto
    {
        public required string Season { get; set; }
    }
}
