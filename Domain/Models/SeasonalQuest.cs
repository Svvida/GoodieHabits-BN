using Domain.Common;

namespace Domain.Models
{
    public class SeasonalQuest : QuestBase
    {
        public int SeasonalQuestId { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }
        public bool IsActive { get; private set; }

        public ICollection<UserSeasonalQuest> UserSeasonalQuests { get; set; } = new List<UserSeasonalQuest>();

        public SeasonalQuest() { }
        public SeasonalQuest(int seasonalQuestId,
            string title,
            string description,
            DateTime activeFrom,
            DateTime activeTo)
        {
            SeasonalQuestId = seasonalQuestId;
            Title = title;
            Description = description;
            ActiveFrom = activeFrom;
            ActiveTo = activeTo;
        }
    }

}
