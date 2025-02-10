using Domain.Common;
using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Models
{
    public class MonthlyQuest : QuestBase
    {
        public required int StartDay { get; set; }
        public required int EndDay { get; set; }
        public MonthlyQuest() : base() { }
        public MonthlyQuest(int id, string title, int startDay, int endDay, PriorityEnum? priority, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate, priority)
        {
            ValidateDays(startDay, endDay);

            StartDay = startDay;
            EndDay = endDay;
        }

        private static void ValidateDays(int startDay, int endDay)
        {
            if (startDay < 1 || startDay > 31)
            {
                throw new InvalidArgumentException($"Start day {startDay} is out of valid range (1-31).");
            }

            if (endDay < 1 || endDay > 31)
            {
                throw new InvalidArgumentException($"End day {endDay} is out of valid range (1-31).");
            }

            if (startDay > endDay)
            {
                throw new InvalidArgumentException($"Start day {startDay} cannot be greater than end day {endDay}.");
            }
        }
    }
}
