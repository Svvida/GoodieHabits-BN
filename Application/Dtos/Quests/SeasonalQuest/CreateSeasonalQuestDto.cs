using Domain.Enum;

namespace Application.Dtos.Quests.SeasonalQuest
{
    public class CreateSeasonalQuestDto : BaseCreateQuestDto
    {
        public string Season { get; set; } = null!;
        public override QuestTypeEnum QuestType { get; set; } = QuestTypeEnum.Seasonal;
    }
}
