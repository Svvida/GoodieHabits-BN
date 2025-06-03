namespace Application.Dtos.Profiles
{
    public class QuestStatsDto
    {
        public int TotalCreated { get; set; }// All-time created
        public int Completed { get; set; }// All-time completed
        public int ExistingQuests { get; set; }
        public int CompletedExistingQuests { get; set; }
    }

}
