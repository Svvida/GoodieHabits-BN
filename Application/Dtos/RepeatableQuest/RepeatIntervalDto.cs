using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enum;

namespace Application.Dtos.RepeatableQuest
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DailyRepeatIntervalDto), "Daily")]
    [JsonDerivedType(typeof(WeeklyRepeatIntervalDto), "Weekly")]
    [JsonDerivedType(typeof(MonthlyRepeatIntervalDto), "Monthly")]
    public abstract class RepeatIntervalDto
    {
        [JsonIgnore] // Prevent duplicate serialization in JSON
        public string Type { get; protected set; } = null!;
    }

    public class DailyRepeatIntervalDto : RepeatIntervalDto
    {
        public DailyRepeatIntervalDto() { Type = "Daily"; }
    }

    public class WeeklyRepeatIntervalDto : RepeatIntervalDto
    {
        public WeeklyRepeatIntervalDto() { Type = "Weekly"; }

        [MinLength(1, ErrorMessage = "At least one day must be selected.")]
        public List<WeekdayEnum> Days { get; set; } = new();
    }

    public class MonthlyRepeatIntervalDto : RepeatIntervalDto
    {
        public MonthlyRepeatIntervalDto() { Type = "Monthly"; }

        [Range(1, 31, ErrorMessage = "Day of the month must be between 1 and 31.")]
        public int? DayOfMonth { get; set; }
        public DateOnly? RepeatFrom { get; set; }
        public DateOnly? RepeatTo { get; set; }
    }
}
