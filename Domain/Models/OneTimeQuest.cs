﻿using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class OneTimeQuest : QuestBase
    {
        public PriorityEnum? Priority { get; set; } = null;
        public OneTimeQuest() : base() { }
        public OneTimeQuest(int id, string title, PriorityEnum? priority, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate)
        {
            Priority = priority;
        }
    }
}
