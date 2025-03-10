using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using OneOf;

namespace Application.Dtos.Quests
{
    [GenerateOneOf]
    public partial class QuestDto : OneOfBase<
        GetOneTimeQuestDto,
        GetDailyQuestDto,
        GetWeeklyQuestDto,
        GetMonthlyQuestDto,
        GetSeasonalQuestDto>
    {
        public static QuestDto From(GetOneTimeQuestDto dto) => new(dto);
        public static QuestDto From(GetDailyQuestDto dto) => new(dto);
        public static QuestDto From(GetWeeklyQuestDto dto) => new(dto);
        public static QuestDto From(GetMonthlyQuestDto dto) => new(dto);
        public static QuestDto From(GetSeasonalQuestDto dto) => new(dto);
    }
}
