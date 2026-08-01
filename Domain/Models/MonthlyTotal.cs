using Domain.Enums;

namespace Domain.Models
{
    /// <summary>
    /// One month's netted total for a single transaction type, as produced by a grouped SQL projection.
    /// Not an entity — a read-model row, used to fold the opening balance without loading transactions.
    /// </summary>
    public record MonthlyTotal(int Year, int Month, FinanceTransactionTypeEnum Type, decimal Total);
}
