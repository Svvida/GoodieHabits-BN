using System.Text.Json.Serialization;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Domain.Enum;

namespace Application.Dtos.Quests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "_dtoType")]
    [JsonDerivedType(typeof(CreateOneTimeQuestDto), "CreateOneTimeQuestDto")]
    [JsonDerivedType(typeof(CreateDailyQuestDto), "CreateDailyQuestDto")]
    [JsonDerivedType(typeof(CreateWeeklyQuestDto), "CreateWeeklyQuestDto")]
    [JsonDerivedType(typeof(CreateMonthlyQuestDto), "CreateMonthlyQuestDto")]
    [JsonDerivedType(typeof(CreateSeasonalQuestDto), "CreateSeasonalQuestDto")]
    public abstract class BaseCreateQuestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public string? Priority { get; set; } = null;
        public string? Difficulty { get; set; } = null;
        public TimeOnly? ScheduledTime { get; set; } = null;
        public HashSet<int> Labels { get; set; } = [];
        [JsonIgnore]
        public int AccountId { get; set; }
        [JsonIgnore]
        public virtual QuestTypeEnum QuestType { get; set; }
    }
}
