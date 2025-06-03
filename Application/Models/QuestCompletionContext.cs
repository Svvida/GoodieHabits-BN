using Domain.Models;
using NodaTime;

namespace Application.Models
{
    internal class QuestCompletionContext
    {
        public Instant NowUtc { get; set; }
        public bool JustCompleted { get; set; }
        public bool ShouldIncrementCount { get; set; }
        public DateTimeZone UserTimeZone { get; set; } = null!;
        public QuestOccurrence? Occurrence { get; set; }
    }
}
