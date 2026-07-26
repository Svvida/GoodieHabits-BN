using Domain.Enums;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public record UpdateTransactionRequest(
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        DateOnly OccurredOn,
        int? CategoryId,
        string? Note);
}
