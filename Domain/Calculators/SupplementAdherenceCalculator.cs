using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Pure adherence arithmetic for a supplement plan: planned doses versus doses actually logged.
    /// <para>
    /// The denominator rule is borrowed verbatim from quest <c>CompletionRate</c> (ARCHITECTURE §6): a day
    /// counts against the user only once it has <em>fully elapsed</em> relative to their local today, or once
    /// something was already taken that day. Without it, opening the app at 09:00 would report a third of your
    /// plan already missed — the dashboard would be wrong at exactly the moment it is being read.
    /// </para>
    /// </summary>
    public static class SupplementAdherenceCalculator
    {
        /// <summary>
        /// Days in the inclusive range that count toward the denominator: strictly before
        /// <paramref name="today"/>, or already satisfied by an intake.
        /// </summary>
        public static int CountEvaluatedDays(
            DateOnly from, DateOnly to, DateOnly today, IReadOnlySet<DateOnly> daysWithIntake)
        {
            ArgumentNullException.ThrowIfNull(daysWithIntake);

            if (to < from)
                return 0;

            var evaluated = 0;

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (day < today || daysWithIntake.Contains(day))
                    evaluated++;
            }

            return evaluated;
        }

        /// <summary>
        /// <paramref name="slotsPerDay"/> × <paramref name="evaluatedDays"/> is the denominator;
        /// <paramref name="takenCount"/> the numerator. The rate is null when nothing has been evaluated, and
        /// is not clamped at 100 — logging extra doses is a real thing the user did and hiding it would make
        /// the number a judgement rather than a measurement.
        /// </summary>
        public static SupplementAdherence Calculate(int slotsPerDay, int evaluatedDays, int takenCount)
        {
            if (slotsPerDay < 0 || evaluatedDays < 0 || takenCount < 0)
                throw new ArgumentOutOfRangeException(nameof(takenCount), "Adherence inputs cannot be negative.");

            var scheduled = slotsPerDay * evaluatedDays;

            var rate = scheduled > 0
                ? Math.Round(takenCount / (decimal)scheduled * 100m, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            return new SupplementAdherence(scheduled, takenCount, rate);
        }
    }
}
