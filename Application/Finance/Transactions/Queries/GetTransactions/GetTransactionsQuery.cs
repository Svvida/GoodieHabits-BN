using Application.Common.Dtos;
using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;
using Domain.Enums;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    public record GetTransactionsQuery(
        int UserProfileId,
        DateOnly? From,
        DateOnly? To,
        FinanceTransactionTypeEnum? Type,
        int? CategoryId,
        int Page,
        int PageSize) : IQuery<PagedResult<TransactionDto>>;
}
