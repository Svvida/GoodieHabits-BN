using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;
using Domain.Enums;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public record UpdateTransactionCommand(
        int TransactionId,
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        DateOnly OccurredOn,
        int? CategoryId,
        string? Note,
        int UserProfileId) : ICommand<TransactionDto>;
}
