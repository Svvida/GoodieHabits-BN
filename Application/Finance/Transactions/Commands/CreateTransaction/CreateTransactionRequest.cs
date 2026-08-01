using Domain.Enums;

namespace Application.Finance.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionRequest(
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        DateOnly OccurredOn,
        int? CategoryId,
        string? Note,
        bool? IsPaid = null);
}
