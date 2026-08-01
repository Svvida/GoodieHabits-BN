using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;
using Domain.Enums;

namespace Application.Finance.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionCommand(
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        DateOnly OccurredOn,
        int? CategoryId,
        string? Note,
        bool? IsPaid,
        int UserProfileId) : ICommand<TransactionDto>;
}
