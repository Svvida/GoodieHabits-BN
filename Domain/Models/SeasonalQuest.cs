using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class SeasonalQuest : QuestBase
    {
        public SeasonEnum Season { get; set; }
        public SeasonalQuest() : base() { }
        public SeasonalQuest(int id, string title, SeasonEnum season, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate)
        {
            Season = season;
        }
    }

}
