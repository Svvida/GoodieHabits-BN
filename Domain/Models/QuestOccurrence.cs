namespace Domain.Models
{
    public class QuestOccurrence
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public DateTime OccurrenceStart { get; private set; }
        public DateTime OccurrenceEnd { get; private set; }
        public bool WasCompleted { get; private set; } = false;
        public DateTime? CompletedAt { get; private set; } = null;

        public Quest Quest { get; private set; } = null!;

        protected QuestOccurrence() { }
        private QuestOccurrence(Quest quest, DateTime occurrenceStart, DateTime occurenceEnd)
        {
            Quest = quest;
            OccurrenceStart = occurrenceStart;
            OccurrenceEnd = occurenceEnd;
        }

        public static QuestOccurrence Create(Quest quest, DateTime occurrenceStart, DateTime occurrenceEnd)
        {
            return new QuestOccurrence(quest, occurrenceStart, occurrenceEnd);
        }

        public void MarkAsCompleted(DateTime completedAt)
        {
            WasCompleted = true;
            CompletedAt = completedAt;
        }

        public void MarkAsIncompleted()
        {
            WasCompleted = false;
            CompletedAt = null;
        }
    }
}
