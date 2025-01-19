﻿using Domain.Common;

namespace Domain.Models
{
    public class RecurringQuest : BaseEntity
    {
        public int RecurringQuestId { get; set; }
        public int AccountId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public TimeOnly RepeatTime { get; set; }
        public required string RepeatIntervalJson { get; set; }
        public string? Emoji { get; set; } = null;
        public bool IsImportant { get; set; } = false;

        public Account Account { get; set; } = null!;
        public RecurringQuest() { }
        public RecurringQuest(int recurringQuestId,
            int accountId,
            string title,
            string description,
            TimeOnly repeatTime,
            string repeatIntervalJson)
        {
            RecurringQuestId = recurringQuestId;
            AccountId = accountId;
            Title = title;
            Description = description;
            RepeatTime = repeatTime;
            RepeatIntervalJson = repeatIntervalJson;
        }
    }
}