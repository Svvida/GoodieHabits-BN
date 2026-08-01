namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    /// <summary>
    /// Partial update — every field is optional and an omitted one is left unchanged. This deliberately differs
    /// from <c>PUT /transactions/{id}</c> (full replacement) and matches <c>UpdateBudgetRequest</c>; the client
    /// contract was written this way and it is the friendlier shape for a settings-style resource.
    /// </summary>
    public record UpdateRecurringTransactionRequest(
        decimal? Amount,
        string? Note,
        int? DayOfMonth,
        bool? IsActive);
}
