using Domain.Models;

namespace Domain.Calculators
{
    /// <summary>
    /// Estimated one-rep max via the Epley formula: <c>weight × (1 + reps / 30)</c>.
    /// <para>
    /// Computed server-side deliberately, so every client shows the same number for the same set — an estimate
    /// that two screens disagree about is worse than no estimate.
    /// </para>
    /// </summary>
    public static class OneRepMaxCalculator
    {
        /// <summary>
        /// Above this rep count the estimate stops meaning anything (Epley is fitted to low-rep sets), so the
        /// calculator returns null rather than a confident-looking number.
        /// </summary>
        public const int MaxMeaningfulReps = 30;

        /// <summary>
        /// Returns the estimate, or null when the set carries no usable reps/weight pair. A single rep returns
        /// the weight itself rather than the formula's 3% inflation — a 1-rep set <em>is</em> the measurement.
        /// </summary>
        public static decimal? Estimate(int? reps, decimal? weight)
        {
            if (reps is not int r || weight is not decimal w)
                return null;

            if (r <= 0 || w <= 0 || r > MaxMeaningfulReps)
                return null;

            if (r == 1)
                return decimal.Round(w, 2, MidpointRounding.AwayFromZero);

            return decimal.Round(w * (1m + r / 30m), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Best estimate across a group of sets, ignoring warm-ups. Null when none of them qualify.</summary>
        public static decimal? BestEstimate(IEnumerable<WorkoutSet> sets, bool includeWarmups = false)
        {
            ArgumentNullException.ThrowIfNull(sets);

            // Max over decimal? ignores nulls and returns null for an empty sequence.
            return sets
                .Where(s => includeWarmups || !s.IsWarmup)
                .Select(s => Estimate(s.Reps, s.Weight))
                .Max();
        }
    }
}
