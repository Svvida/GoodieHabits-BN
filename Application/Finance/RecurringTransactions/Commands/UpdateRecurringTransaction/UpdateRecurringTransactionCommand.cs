using Application.Common.Interfaces;
using Application.Finance.RecurringTransactions.Dtos;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    /// <summary>
    /// <paramref name="HasCategoryId"/> carries the request's field presence through to the handler: with it
    /// false the category is left alone, with it true a null <paramref name="CategoryId"/> clears the category.
    /// </summary>
    public record UpdateRecurringTransactionCommand(
        int RecurringTransactionId,
        decimal? Amount,
        string? Note,
        int? DayOfMonth,
        bool? IsActive,
        int? CategoryId,
        bool HasCategoryId,
        int UserProfileId) : ICommand<RecurringTransactionDto>;
}
