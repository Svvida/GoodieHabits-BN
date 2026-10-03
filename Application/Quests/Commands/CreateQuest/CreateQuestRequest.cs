using Application.Quests.Dtos;

namespace Application.Quests.Commands.CreateQuest
{
    /// <summary>Body of <c>POST /api/quests</c>.</summary>
    public record CreateQuestRequest
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
