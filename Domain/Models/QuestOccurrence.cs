namespace Domain.Models
{
    public class QuestOccurrence
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public DateTime OccurenceStart { get; set; }
        public DateTime OccurenceEnd { get; set; }
        public bool WasCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; } = null;

        public Quest Quest { get; set; } = null!;

        public QuestOccurrence() { }
        public QuestOccurrence(int questId, DateTime occurrenceStart, DateTime occurenceEnd)
        {
            QuestId = questId;
            OccurenceStart = occurrenceStart;
            OccurenceEnd = occurenceEnd;
        }
    }
}
