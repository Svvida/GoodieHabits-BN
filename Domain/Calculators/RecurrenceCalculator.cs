using Domain.Models;

namespace Domain.Calculators
{
    /// <summary>
    /// Works out which months a recurring template still owes transactions for, and on which date each lands.
    /// Pure and DB-free — the <c>QuestWindowCalculator</c> analogue for finance.
    /// <para>
    /// Construction of the transactions themselves stays in the handler: unlike <c>Quest</c> and its
    /// occurrences, a <see cref="RecurringTransaction"/> does not own the <see cref="FinanceTransaction"/> rows
    /// it produces — they are separate aggregates with independent lifecycles.
    /// </para>
    /// </summary>
    public static class RecurrenceCalculator
    {
        /// <summary>
        /// The dates a template should be materialized on: one per month from just after its watermark up to
        /// and including <paramref name="today"/>'s month, each clamped to the month's length. Never returns a
        /// future month. Empty when the template is up to date or inactive.
        /// </summary>
        public static IReadOnlyList<DateOnly> GetMissingOccurrences(RecurringTransaction template, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(template);

            if (!template.IsActive)
                return [];

            var currentMonth = new DateOnly(today.Year, today.Month, 1);

            // A null watermark shouldn't happen (Create stamps it), but treating it as "this month is already
            // covered" is the safe default: it can only ever under-generate, never duplicate a user's row.
            var watermark = template.LastMaterializedOn is DateOnly last
                ? new DateOnly(last.Year, last.Month, 1)
                : currentMonth;

            if (watermark >= currentMonth)
                return [];

            var occurrences = new List<DateOnly>();
            for (var month = watermark.AddMonths(1); month <= currentMonth; month = month.AddMonths(1))
                occurrences.Add(ClampToMonth(month.Year, month.Month, template.DayOfMonth));

            return occurrences;
        }

        /// <summary>
        /// The template's day within a given month, clamped to the last day when the month is too short
        /// (day 31 in February becomes the 28th, or the 29th in a leap year).
        /// </summary>
        public static DateOnly ClampToMonth(int year, int month, int dayOfMonth)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            return new DateOnly(year, month, Math.Min(dayOfMonth, daysInMonth));
        }
    }
}
