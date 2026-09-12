using Application.Common.Dtos;
using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;
using Domain.Enums;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    /// <summary>
    /// <paramref name="CategoryIds"/> is an OR-set, and a main category in it stands for its sub-categories
    /// too (the handler expands it) — pass the sub's own id to narrow to just that sub.
    /// <paramref name="Search"/> is free text matched against the note, the category name and the row's own
    /// corrections' notes. Every filter narrows the same list rather than replacing the others.
    /// </summary>
    public record GetTransactionsQuery(
        int UserProfileId,
        DateOnly? From,
        DateOnly? To,
        FinanceTransactionTypeEnum? Type,
        IReadOnlyList<int>? CategoryIds,
        bool? IsPaid,
        string? Search,
        int Page,
        int PageSize) : IQuery<PagedResult<TransactionDto>>;
}
