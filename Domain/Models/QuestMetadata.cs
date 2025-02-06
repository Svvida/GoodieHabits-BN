using Domain.Enum;

namespace Domain.Models
{
    public class QuestMetadata
    {
        public int Id { get; set; }
        public QuestTypeEnum QuestType { get; set; }
        public required int AccountId { get; set; }
        public Account Account { get; set; } = null!;
        public OneTimeQuest? OneTimeQuest { get; set; }
        public DailyQuest? DailyQuest { get; set; }
        public WeeklyQuest? WeeklyQuest { get; set; }
        public MonthlyQuest? MonthlyQuest { get; set; }
        public SeasonalQuest? SeasonalQuest { get; set; }
        public QuestMetadata() { }
        public QuestMetadata(int id, QuestTypeEnum type, int accountId)
        {
            Id = id;
            QuestType = type;
            AccountId = accountId;
        }
    }
}
