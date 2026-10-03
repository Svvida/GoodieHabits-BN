using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Commands.CreateQuest
{
    /// <summary>
    /// Creates any quest. This single command replaces the five <c>Create{OneTime,Daily,Weekly,Monthly,
    /// Seasonal}QuestCommand</c> records and their parallel handlers, validators and DTOs — the shape they
    /// varied on is now data, in <see cref="Schedule"/>.
    /// </summary>
    public record CreateQuestCommand : ICommand<QuestDetailsDto>
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
        public int UserProfileId { get; init; }
    }
}
