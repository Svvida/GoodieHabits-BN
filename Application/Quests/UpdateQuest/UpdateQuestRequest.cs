namespace Application.Quests.UpdateQuest
{
    public abstract record UpdateQuestRequest
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; } = null;
        public DateTime? StartDate { get; init; } = null;
        public DateTime? EndDate { get; init; } = null;
        public string? Emoji { get; init; } = null;
        public string? Priority { get; init; } = null;
        public string? Difficulty { get; init; } = null;
        public TimeOnly? ScheduledTime { get; init; } = null;
        public HashSet<int> Labels { get; init; } = [];
    }

    public record UpdateOneTimeQuestRequest : UpdateQuestRequest;
    public record UpdateDailyQuestRequest : UpdateQuestRequest;

    public record UpdateWeeklyQuestRequest : UpdateQuestRequest
    {
        public HashSet<string> Weekdays { get; init; } = [];
    }

    public record UpdateMonthlyQuestRequest : UpdateQuestRequest
    {
        public int StartDay { get; init; }
        public int EndDay { get; init; }
    }

    public record UpdateSeasonalQuestRequest : UpdateQuestRequest
    {
        public string Season { get; init; } = null!;
    }
}