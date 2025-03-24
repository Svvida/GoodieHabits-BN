using Domain.Enum;
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
        public PriorityEnum? Priority { get; set; } = null;
        public QuestMetadata QuestMetadata { get; set; } = null!;

        public QuestBase() { }
        public QuestBase(int id, string title, string? description, string? emoji, DateTime? startDate, DateTime? endDate, PriorityEnum? priority)
        {
            Id = id;
            Title = title;
            Description = description;
            Emoji = emoji;
            StartDate = startDate;
            EndDate = endDate;
            Priority = priority;
        }

        public void UpdateDates(DateTime? newStartDate, DateTime? newEndDate)
        {
            if (newStartDate.HasValue && newEndDate.HasValue)
            {
                StartDate = newStartDate;
                EndDate = newEndDate;
            }
            if (newStartDate.HasValue && EndDate.HasValue && newStartDate > EndDate)
            {
                throw new InvalidArgumentException("Start date cannot be after the end date.");
            }

            if (newEndDate.HasValue && StartDate.HasValue && newEndDate < StartDate)
            {
                throw new InvalidArgumentException("End date cannot be before the start date.");
            }

            StartDate = newStartDate ?? StartDate;
            EndDate = newEndDate ?? EndDate;
        }
    }
}
