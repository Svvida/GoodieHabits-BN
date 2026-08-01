namespace Domain.ValueObjects
{
    /// <summary>
    /// A calendar period a repeatable quest can be completed in, expressed in the user's local
    /// calendar. Both bounds are inclusive.
    /// </summary>
    public record QuestOccurrenceWindow(DateOnly Start, DateOnly End);
}
