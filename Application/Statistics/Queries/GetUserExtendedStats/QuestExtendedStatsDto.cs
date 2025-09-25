namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public class QuestExtendedStatsDto
    {
        public int TotalCreated { get; set; }
        public int TotalCompleted { get; set; }
        public int CurrentTotal { get; set; }
        public int CurrentEverCompleted { get; set; }
        public int CurrentCompleted { get; set; }
        public int CompletedDaily { get; set; }
        public int CompletedWeekly { get; set; }
        public int CompletedMonthly { get; set; }
    }
}
