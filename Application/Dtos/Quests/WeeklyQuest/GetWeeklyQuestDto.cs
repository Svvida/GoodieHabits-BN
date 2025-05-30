using Application.Dtos.RepeatableQuestsStatistics;

namespace Application.Dtos.Quests.WeeklyQuest
{
    public class GetWeeklyQuestDto : BaseGetQuestDto
    {
        public List<string> Weekdays { get; set; } = [];
        public RepeatableQuestStatisticsDto Statistics { get; set; } = new RepeatableQuestStatisticsDto();
    }
}
