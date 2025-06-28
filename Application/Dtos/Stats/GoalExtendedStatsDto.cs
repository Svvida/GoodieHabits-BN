namespace Application.Dtos.Stats
{
    public class GoalExtendedStatsDto
    {
        public int CurrentTotal { get; set; }
        public int CurrentCompleted { get; set; }
        public int InProgress { get; set; }
        public int TotalCreated { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalExpired { get; set; }
    }
}
