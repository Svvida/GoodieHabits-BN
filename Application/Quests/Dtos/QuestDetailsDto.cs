using System.Text.Json.Serialization;
using Application.QuestLabels.Dtos;

namespace Application.Quests.Dtos
{
    [JsonDerivedType(typeof(OneTimeQuestDetailsDto), typeDiscriminator: "OneTime")]
    [JsonDerivedType(typeof(DailyQuestDetailsDto), typeDiscriminator: "Daily")]
    [JsonDerivedType(typeof(WeeklyQuestDetailsDto), typeDiscriminator: "Weekly")]
    [JsonDerivedType(typeof(MonthlyQuestDetailsDto), typeDiscriminator: "Monthly")]
    [JsonDerivedType(typeof(SeasonalQuestDetailsDto), typeDiscriminator: "Seasonal")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "questType")]
    public abstract record QuestDetailsDto
    {
        public int Id { get; init; }
        public string QuestType { get; init; } = null!;
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? Emoji { get; init; }
        public bool IsCompleted { get; init; }
        public string? Priority { get; init; }
        public string? Type { get; init; }
        public string? Difficulty { get; init; }
        public TimeOnly? ScheduledTime { get; init; }
        public DateTime? LastCompletedAt { get; init; }
        public ICollection<QuestLabelDto> Labels { get; init; } = [];
    }

    public record OneTimeQuestDetailsDto : QuestDetailsDto;
    public record DailyQuestDetailsDto : QuestDetailsDto
    {
        public RepeatableQuestStatisticsDto Statistics { get; init; } = null!;
    }
    public record WeeklyQuestDetailsDto : QuestDetailsDto
    {
        public RepeatableQuestStatisticsDto Statistics { get; init; } = null!;
        public HashSet<string> Weekdays { get; init; } = [];
    }
    public record MonthlyQuestDetailsDto : QuestDetailsDto
    {
        public RepeatableQuestStatisticsDto Statistics { get; init; } = null!;
        public int StartDay { get; init; }
        public int EndDay { get; init; }
    }
    public record SeasonalQuestDetailsDto : QuestDetailsDto
    {
        public string Season { get; init; } = null!;
    }
}
