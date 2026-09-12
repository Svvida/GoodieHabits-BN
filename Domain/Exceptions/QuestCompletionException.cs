namespace Domain.Exceptions
{
    /// <summary>
    /// A completion the quest's own rules refuse: too many for one day, absurdly far over the target, or
    /// dated outside the catch-up window. Distinct from a validation failure because the decision needs the
    /// quest's periods and existing completions, which only the entity has.
    /// </summary>
    public class QuestCompletionException(string message, int statusCode = 409) : AppException(message, statusCode)
    {
    }
}
