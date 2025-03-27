using Domain.Enum;

namespace Domain.Models
{
    public class SeasonalQuest_Season
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public SeasonEnum Season { get; set; }

        public Quest Quest { get; set; } = null!;

        public SeasonalQuest_Season() { }
        public SeasonalQuest_Season(int id, int questId, SeasonEnum season)
        {
            Id = id;
            QuestId = questId;
            Season = season;
        }
    }
}
