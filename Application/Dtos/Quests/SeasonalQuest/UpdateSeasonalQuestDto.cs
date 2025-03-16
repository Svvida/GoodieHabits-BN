namespace Application.Dtos.Quests.SeasonalQuest
{
    public class UpdateSeasonalQuestDto : BaseUpdateQuestDto
    {
        public string Season { get; set; } = null!;
    }
}
