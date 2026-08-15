namespace Domain.ValueObjects
{
    /// <summary>
    /// Allow-list of weight units the workouts module accepts. A single unit is stored per user on
    /// <c>UserProfile.WeightUnit</c>, exactly like <c>UserProfile.Currency</c> — and, exactly like currency,
    /// changing it does <em>not</em> convert historical values. Weights are stored as the user typed them.
    /// <para>
    /// A per-set unit column was considered and rejected: it doubles the width of the module's largest table to
    /// serve a case that does not occur (nobody logs half a session in pounds).
    /// </para>
    /// </summary>
    public static class SupportedWeightUnits
    {
        public const string Default = "kg";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kg", "lb",
        };

        public static bool IsSupported(string? unit)
            => !string.IsNullOrWhiteSpace(unit) && All.Contains(unit.Trim());
    }
}
