using Domain.Common;

namespace Domain.Models
{
    public class SeasonalQuest : BaseEntity
    {
        public int SeasonalQuestId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }
        public string? Emoji { get; set; } = null;
        public bool IsImportant { get; set; } = false;

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
