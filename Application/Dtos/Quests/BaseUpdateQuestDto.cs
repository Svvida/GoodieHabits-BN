using System.Text.Json.Serialization;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;

namespace Application.Dtos.Quests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "_dtoType")]
    [JsonDerivedType(typeof(UpdateOneTimeQuestDto), "UpdateOneTimeQuestDto")]
    [JsonDerivedType(typeof(UpdateDailyQuestDto), "UpdateDailyQuestDto")]
    [JsonDerivedType(typeof(UpdateWeeklyQuestDto), "UpdateWeeklyQuestDto")]
    [JsonDerivedType(typeof(UpdateMonthlyQuestDto), "UpdateMonthlyQuestDto")]
    [JsonDerivedType(typeof(UpdateSeasonalQuestDto), "UpdateSeasonalQuestDto")]
    public abstract class BaseUpdateQuestDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public string? Priority { get; set; }
        public string? Difficulty { get; set; }
        public TimeOnly? ScheduledTime { get; set; }
        public HashSet<int> Labels { get; set; } = [];

        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
