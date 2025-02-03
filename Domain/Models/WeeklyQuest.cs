using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Domain.Common;
using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Models
{
    public class WeeklyQuest : QuestBase
    {
        public PriorityEnum? Priority { get; set; } = null;
        [NotMapped]
        public List<WeekdayEnum> Weekdays { get; set; } = [];

        // This property is used to store the Weekdays list in the database as a string
        public string WeekdaysSerialized
        {
            get => JsonSerializer.Serialize(Weekdays); // Convert List<Enum> to JSON string before saving to database
            set => Weekdays = string.IsNullOrEmpty(value)  // Convert JSON string to List<Enum> after reading from database
                ? new List<WeekdayEnum>()
                : JsonSerializer.Deserialize<List<WeekdayEnum>>(value)!;
        }

        public WeeklyQuest() : base() { }

        public WeeklyQuest(int id, string title, List<WeekdayEnum> weekdays, PriorityEnum? priority, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate)
        {
            if (weekdays.Count == 0)
            {
                throw new InvalidArgumentException("Weekdays cannot be empty.");
            }

            Weekdays = weekdays;
            Priority = priority;
        }
    }
}
