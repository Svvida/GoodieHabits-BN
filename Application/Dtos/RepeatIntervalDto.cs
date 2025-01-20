namespace Application.Dtos
{
    public abstract class RepeatIntervalDto
    {
        public string Type { get; set; } = null!;
    }

    public class DailyRepeatIntervalDto : RepeatIntervalDto { }

    public class WeeklyRepeatIntervalDto : RepeatIntervalDto
    {
        public List<string> Days { get; set; } = new();
        public int? Frequency { get; set; }
    }

    public class MonthlyRepeatIntervalDto : RepeatIntervalDto
    {
        public int? DayOfMonth { get; set; }
        public string? Day { get; set; }
        public int? Frequency { get; set; }
    }
}
