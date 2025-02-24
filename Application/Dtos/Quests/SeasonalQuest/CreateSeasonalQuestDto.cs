namespace Application.Dtos.Quests.SeasonalQuest
{
    public class CreateSeasonalQuestDto : BaseCreateQuestDto
    {
        public required string Season { get; set; }
    }
}
