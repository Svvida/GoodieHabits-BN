using Domain.Exceptions;

namespace Domain.Models
{
    public class MonthlyQuest_Days
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public int StartDay { get; set; }
        public int EndDay { get; set; }

        public Quest Quest { get; set; } = null!;

        public MonthlyQuest_Days() { }
        public MonthlyQuest_Days(int id, int questId, int startDay, int endDay)
        {
            ValidateDays(startDay, endDay);

            Id = id;
            QuestId = questId;
            StartDay = startDay;
            EndDay = endDay;
        }

        private void ValidateDays(int startDay, int endDay)
        {
            if (startDay < 1 || startDay > 31)
                throw new InvalidArgumentException("Start day must be between 1 and 31.");
            if (endDay < 1 || endDay > 31)
                throw new InvalidArgumentException("End day must be between 1 and 31.");
            if (startDay > endDay)
                throw new InvalidArgumentException("Start day cannot be after the end day.");
        }
    }
}
