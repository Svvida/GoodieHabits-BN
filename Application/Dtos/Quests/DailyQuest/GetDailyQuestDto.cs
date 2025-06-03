namespace Application.Dtos.Quests.DailyQuest
{
    public class GetDailyQuestDto : BaseGetQuestDto
    {
        public RepeatableQuestStatisticsDto Statistics { get; set; } = new RepeatableQuestStatisticsDto();
    }
}
