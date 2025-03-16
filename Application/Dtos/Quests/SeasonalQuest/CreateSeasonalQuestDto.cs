namespace Application.Dtos.Quests.SeasonalQuest
{
    public class CreateSeasonalQuestDto : BaseCreateQuestDto
    {
        public string Season { get; set; } = null!;
    }
}
