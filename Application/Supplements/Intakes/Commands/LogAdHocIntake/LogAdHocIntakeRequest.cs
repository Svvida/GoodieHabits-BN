namespace Application.Supplements.Intakes.Commands.LogAdHocIntake
{
    /// <summary>
    /// A dose taken outside the plan — "wziąłem dziś jeszcze jedną".
    /// <para>
    /// Deliberately a <c>POST</c> and deliberately <b>not</b> idempotent, unlike the checkbox: repeating an
    /// unplanned dose is a real event, and collapsing two of them into one would lose a fact the user chose to
    /// record. Undo is <c>DELETE /supplements/intakes/{id}</c>.
    /// </para>
    /// <para><c>amount</c> falls back to the supplement's default amount.</para>
    /// </summary>
    public record LogAdHocIntakeRequest(
        int SupplementId,
        DateOnly Date,
        decimal? Amount = null,
        int? WorkoutSessionId = null);
}
