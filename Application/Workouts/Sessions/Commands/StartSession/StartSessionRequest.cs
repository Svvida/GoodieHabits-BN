namespace Application.Workouts.Sessions.Commands.StartSession
{
    /// <summary>
    /// Starts a session. Either give a <see cref="RoutineId"/> (the session is materialized from that template)
    /// or a <see cref="Name"/> for an ad-hoc session; with a routine, the name defaults to the routine's.
    /// <para>
    /// <see cref="PerformedOn"/> is optional and defaults to the user's local today, derived from their
    /// profile timezone. Send it explicitly when back-filling a session logged on paper.
    /// </para>
    /// </summary>
    public record StartSessionRequest(
        int? RoutineId = null,
        string? Name = null,
        DateOnly? PerformedOn = null,
        string? Note = null);
}
