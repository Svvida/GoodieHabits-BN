using Application.Common.Interfaces;
using Application.Finance.RecurringTransactions.Dtos;
using Domain.Enums;

namespace Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction
{
    public record CreateRecurringTransactionCommand(
        FinanceTransactionTypeEnum Type,
        decimal Amount,
        int DayOfMonth,
        int? CategoryId,
        string? Note,
        int UserProfileId) : ICommand<RecurringTransactionDto>;
}
