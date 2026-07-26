using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Pure comparison of actual spending against a planned limit. Side-effect free.
    /// </summary>
    public static class BudgetProgressCalculator
    {
        public static BudgetProgress Calculate(decimal limit, decimal spent)
        {
            var remaining = limit - spent;
            var percentUsed = limit > 0 ? Math.Round(spent / limit * 100m, 2, MidpointRounding.AwayFromZero) : 0m;
            return new BudgetProgress(limit, spent, remaining, percentUsed, spent > limit);
        }
    }
}
