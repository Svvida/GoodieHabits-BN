using System.Text.Json.Serialization;

namespace Domain.Models
{
    [JsonDerivedType(typeof(DailyRepeatInterval), "Daily")]
    [JsonDerivedType(typeof(WeeklyRepeatInterval), "Weekly")]
    [JsonDerivedType(typeof(MonthlyRepeatInterval), "Monthly")]
    public abstract class RepeatInterval
    {
        public required string Type { get; set; }
    }

    public class DailyRepeatInterval : RepeatInterval
    {
        public DailyRepeatInterval()
        {
            Type = "Daily";
        }
    }

    public class WeeklyRepeatInterval : RepeatInterval
    {
        public WeeklyRepeatInterval()
        {
            Type = "Weekly";
        }
        public List<string> Days { get; set; } = new();
        public int? Frequency { get; set; } // OptionalL: e.g., every 2 weeks
    }

    public class MonthlyRepeatInterval : RepeatInterval
    {
        public MonthlyRepeatInterval()
        {
            Type = "Monthly";
        }
        // For specific day (e.g., 1st day of the month)
        public int? DayOfMonth { get; set; }
        // For "Monday", "Tesday" etc.
        public string? Day { get; set; }
        // Optional: e.g., every 2nd Monday
        public string? Frequency { get; set; }
    }
}
