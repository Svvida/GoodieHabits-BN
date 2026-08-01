using Application.Common.Interfaces;
using Application.Finance.RecurringTransactions.Dtos;

namespace Application.Finance.RecurringTransactions.Queries.GetRecurringTransactions
{
    public record GetRecurringTransactionsQuery(int UserProfileId) : IQuery<IEnumerable<RecurringTransactionDto>>;
}
