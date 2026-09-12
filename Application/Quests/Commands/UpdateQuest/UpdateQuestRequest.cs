using Application.Quests.Dtos;

namespace Application.Quests.Commands.UpdateQuest
{
    /// <summary>Body of <c>PUT /api/quests/{id}</c>.</summary>
    public record UpdateQuestRequest
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? Emoji { get; init; }
        public string? Priority { get; init; }
        public string? Difficulty { get; init; }
        public TimeOnly? ScheduledTime { get; init; }
        public int? DurationMinutes { get; init; }
        public HashSet<int> Labels { get; init; } = [];
        public QuestScheduleRequest Schedule { get; init; } = new();
        public QuestTargetRequest? Target { get; init; }
    }
}
