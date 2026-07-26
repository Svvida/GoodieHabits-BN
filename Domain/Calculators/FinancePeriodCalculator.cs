using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Calculators
{
    /// <summary>
    /// Turns calendar coordinates (year, optional month) into inclusive <see cref="DateOnly"/> bounds.
    /// Timezone-free by design: finance transaction dates are calendar facts, not instants.
    /// </summary>
    public static class FinancePeriodCalculator
    {
        public static (DateOnly Start, DateOnly End) ForMonth(int year, int month)
        {
            ValidateYear(year);
            if (month is < 1 or > 12)
                throw new InvalidArgumentException("Month must be between 1 and 12.");

            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }

        public static (DateOnly Start, DateOnly End) ForYear(int year)
        {
            ValidateYear(year);
            return (new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));
        }

        public static (DateOnly Start, DateOnly End) ForPeriod(BudgetPeriodEnum period, int year, int? month)
            => period == BudgetPeriodEnum.Monthly
                ? ForMonth(year, month ?? throw new InvalidArgumentException("Monthly period requires a month."))
                : ForYear(year);

        private static void ValidateYear(int year)
        {
            if (year is < 1 or > 9999)
                throw new InvalidArgumentException("Year must be between 1 and 9999.");
        }
    }
}
