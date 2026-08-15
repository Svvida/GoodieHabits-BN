namespace Domain.ValueObjects
{
    /// <summary>
    /// How closely a supplement plan was actually followed over a period.
    /// </summary>
    /// <param name="Scheduled">Planned doses whose day has been evaluated (see the calculator's denominator rule).</param>
    /// <param name="Taken">Doses actually logged against those slots.</param>
    /// <param name="Rate">
    /// Percentage 0-100, or <c>null</c> when nothing has been evaluated yet. Null rather than zero for the same
    /// reason quest <c>CompletionRate</c> is null on an unevaluated quest: "no data" and "you took none of them"
    /// are different statements, and only one of them should colour a dashboard red.
    /// </param>
    public record SupplementAdherence(int Scheduled, int Taken, decimal? Rate);
}
