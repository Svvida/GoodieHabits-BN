namespace Domain.Exceptions
{
    public class NoOccurrenceToMarkAsCompletedException(int questId) : AppException($"There is no occurrence from last 24h to mark as completed for quest with ID {questId}.", 409)
    {
        public int QuestId { get; set; } = questId;
    }
}
