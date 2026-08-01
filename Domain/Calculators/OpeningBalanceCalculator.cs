using Domain.Enums;
using Domain.Models;

namespace Domain.Calculators
{
    /// <summary>
    /// Folds a user's monthly totals into the balance carried <em>into</em> a given month — the money left over
    /// from everything before it.
    /// <para>
    /// The chain starts at the user's earliest month, whose opening balance is 0 by definition, and runs forward
    /// month by month. It is deliberately <b>signed</b>: a month that ends at -500 carries -500 forward.
    /// Clamping to zero was rejected because it both hides the number the user most needs to see and breaks the
    /// chain — an erased shortfall never reappears, so the running balance stops being a balance. Rendering a
    /// negative differently is the client's call.
    /// </para>
    /// </summary>
    public static class OpeningBalanceCalculator
    {
        /// <summary>
        /// The balance carried into (<paramref name="year"/>, <paramref name="month"/>), i.e. the fold over
        /// every month strictly before it. Months with no activity contribute nothing but do not break the
        /// chain. Returns 0 when there is no earlier history.
        /// </summary>
        public static decimal Calculate(IEnumerable<MonthlyTotal> monthlyTotals, int year, int month)
        {
            ArgumentNullException.ThrowIfNull(monthlyTotals);

            var cutoff = (year * 12) + month;
            var balance = 0m;

            foreach (var total in monthlyTotals)
            {
                if ((total.Year * 12) + total.Month >= cutoff)
                    continue;

                balance += total.Type == FinanceTransactionTypeEnum.Income ? total.Total : -total.Total;
            }

            return balance;
        }
    }
}
