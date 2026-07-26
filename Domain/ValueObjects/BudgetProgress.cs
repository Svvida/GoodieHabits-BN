namespace Domain.ValueObjects
{
    /// <summary>
    /// Result of comparing actual spending against a budget limit for a period.
    /// Produced by <see cref="Domain.Calculators.BudgetProgressCalculator"/>.
    /// </summary>
    public sealed record BudgetProgress(
        decimal Limit,
        decimal Spent,
        decimal Remaining,
        decimal PercentUsed,
        bool IsOverBudget);
}
