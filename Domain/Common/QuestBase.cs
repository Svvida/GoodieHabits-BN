using Domain.Exceptions;
using Domain.Models;

namespace Domain.Common
{
    public abstract class QuestBase : EntityBase
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public string? Emoji { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public QuestMetadata QuestMetadata { get; set; } = null!;

        public QuestBase() { }
        public QuestBase(int id, string title, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
        {
            ValidateDates(startDate, endDate);

            Id = id;
            Title = title;
            Description = description;
            Emoji = emoji;
            StartDate = startDate;
            EndDate = endDate;
        }

        private static void ValidateDates(DateTime? startDate, DateTime? endDate)
        {
            if (startDate is not null && endDate is not null && startDate > endDate)
            {
                throw new InvalidArgumentException("Start date must be before end date");
            }
        }
    }
}
