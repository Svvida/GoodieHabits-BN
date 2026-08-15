using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.LogSession
{
    /// <summary>
    /// The whole exercise+set tree for a session, replacing whatever is there.
    /// <para>
    /// This is the offline-friendly path: a phone with no signal in the gym keeps the session in local state
    /// and syncs once. It is <b>full replacement rather than append precisely so a retry is harmless</b> —
    /// sending the same payload twice leaves the same log, not a doubled one.
    /// </para>
    /// <para>An empty array clears the log. That is a legal thing to do, so send the whole tree every time.</para>
    /// </summary>
    public record LogSessionRequest(IReadOnlyList<SessionExerciseInput> Exercises);
}
