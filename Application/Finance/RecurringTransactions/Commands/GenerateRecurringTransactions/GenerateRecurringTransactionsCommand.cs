using Application.Common.Interfaces;

namespace Application.Finance.RecurringTransactions.Commands.GenerateRecurringTransactions
{
    /// <summary>
    /// Catch-up sweep across every user's active templates. Returns the number of rows written, mirroring
    /// <c>GenerateMissingOccurrencesCommand</c>. Correct whenever it happens to run and harmless if it runs
    /// twice, so it needs no scheduler.
    /// </summary>
    public record GenerateRecurringTransactionsCommand : ICommand<int>;
}
