using Application.Common.Interfaces;
using Application.Finance.RecurringTransactions.Dtos;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    public record UpdateRecurringTransactionCommand(
        int RecurringTransactionId,
        decimal? Amount,
        string? Note,
        int? DayOfMonth,
        bool? IsActive,
        int UserProfileId) : ICommand<RecurringTransactionDto>;
}
