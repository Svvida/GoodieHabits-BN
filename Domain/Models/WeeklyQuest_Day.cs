using Domain.Enum;

namespace Domain.Models
{
    public class WeeklyQuest_Day
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public WeekdayEnum Weekday { get; set; }

        public Quest Quest { get; set; } = null!;

        public WeeklyQuest_Day() { }
        public WeeklyQuest_Day(int id, int questId, WeekdayEnum weekday)
        {
            Id = id;
            QuestId = questId;
            Weekday = weekday;
        }
    }
}
