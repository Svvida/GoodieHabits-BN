using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Commands.UpdateQuest
{
    /// <summary>Updates any quest. Replaces the five per-type update commands.</summary>
    public record UpdateQuestCommand : ICommand<QuestDetailsDto>, ICurrentUserQuestCommand
    {
        public int QuestId { get; init; }
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
        public int UserProfileId { get; init; }
    }
}
