namespace Application.Workouts.Sessions.Commands.UpdateSession
{
    /// <summary>
    /// Session metadata only — name, date, note. The exercise log is edited through the log endpoints, never
    /// here, so a client fixing a typo can never wipe a workout by omitting a field.
    /// </summary>
    public record UpdateSessionRequest(string Name, DateOnly PerformedOn, string? Note = null);
}
