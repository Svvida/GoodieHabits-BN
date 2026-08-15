namespace Application.Supplements.Intakes.Commands.ToggleIntake
{
    /// <summary>
    /// The checkbox. The client sends the <b>intent</b> — "this planned dose, on this date, is/isn't taken" —
    /// not a row id it would otherwise have to track.
    /// <para>
    /// Deliberately a <c>PUT</c>: a phone taps fast, offline, and sometimes twice. Setting <c>taken</c> to a
    /// value it already has is a no-op, so a retry can never produce a second dose. The database backs this up
    /// with a unique index on (slot, date).
    /// </para>
    /// <para>
    /// <c>amount</c> is optional and falls back to the slot's planned amount. <c>workoutSessionId</c> is set by
    /// the in-training panel to record that the dose was taken around that session.
    /// </para>
    /// </summary>
    public record ToggleIntakeRequest(
        int SupplementId,
        int SlotId,
        DateOnly Date,
        bool Taken,
        decimal? Amount = null,
        int? WorkoutSessionId = null);
}
