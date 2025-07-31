using System.Text.Json.Serialization;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.QuestLabels.Queries.GetUserLabels;

namespace Application.Dtos.Quests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "_dtoType")]
    [JsonDerivedType(typeof(GetOneTimeQuestDto), "GetOneTimeQuestDto")]
    [JsonDerivedType(typeof(GetDailyQuestDto), "GetDailyQuestDto")]
    [JsonDerivedType(typeof(GetWeeklyQuestDto), "GetWeeklyQuestDto")]
    [JsonDerivedType(typeof(GetMonthlyQuestDto), "GetMonthlyQuestDto")]
    [JsonDerivedType(typeof(GetSeasonalQuestDto), "GetSeasonalQuestDto")]
    public abstract class BaseGetQuestDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public string? Priority { get; set; }
        public string? Type { get; set; }
        public string? Difficulty { get; set; }
        public TimeOnly? ScheduledTime { get; set; }
        public DateTime? LastCompletedAt { get; set; }
        public ICollection<GetQuestLabelDto> Labels { get; set; } = [];
    }
}
