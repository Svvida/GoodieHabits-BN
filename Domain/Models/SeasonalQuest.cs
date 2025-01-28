using Domain.Common;

namespace Domain.Models
{
    public class SeasonalQuest : QuestBase
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Quest Quest { get; set; } = null!;

        public SeasonalQuest() { }
        public SeasonalQuest(int seasonalQuestId,
            string title,
            string description,
            DateTime startDate,
            DateTime endDate,
            bool isCompleted)
        {
            Id = seasonalQuestId;
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            IsCompleted = isCompleted;
        }
    }

}
