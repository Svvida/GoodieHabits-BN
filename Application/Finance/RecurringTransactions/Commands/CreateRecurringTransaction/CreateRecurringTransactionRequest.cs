using Domain.Enums;

namespace Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction
{
    public record CreateRecurringTransactionRequest(
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        int DayOfMonth,
        int? CategoryId,
        string? Note);
}
