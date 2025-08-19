using Domain.Enums;

namespace Domain.Models
{
    public class WeeklyQuest_Day
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public WeekdayEnum Weekday { get; set; }

        public Quest Quest { get; set; } = null!;

        public WeeklyQuest_Day() { }
        public WeeklyQuest_Day(WeekdayEnum weekday)
        {
            Weekday = weekday;
        }
    }
}
