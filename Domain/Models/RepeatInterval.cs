using System.Text.Json.Serialization;
using Domain.Enum;

namespace Domain.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DailyRepeatInterval), "Daily")]
    [JsonDerivedType(typeof(WeeklyRepeatInterval), "Weekly")]
    [JsonDerivedType(typeof(MonthlyRepeatInterval), "Monthly")]
    public abstract class RepeatInterval
    {
        [JsonIgnore] // Prevent duplicate serialization in JSON
        public string Type { get; set; } = null!;
    }

    public class DailyRepeatInterval : RepeatInterval
    {
        public DailyRepeatInterval() => Type = "Daily";
    }

    public class WeeklyRepeatInterval : RepeatInterval
    {
        public WeeklyRepeatInterval() => Type = "Weekly";
        public List<WeekdayEnum> Days { get; set; } = new();
    }

    public class MonthlyRepeatInterval : RepeatInterval
    {
        public MonthlyRepeatInterval() => Type = "Monthly";

        // For specific day (e.g., 1st day of the month)
        public int? DayOfMonth { get; set; }
        // For "Monday", "Tesday" etc.
        //public WeekdayEnum? Day { get; set; }
        public int? RepeatFrom { get; set; }
        public int? RepeatTo { get; set; }
    }
}
