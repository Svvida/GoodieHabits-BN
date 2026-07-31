using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;

namespace Application.Finance.Transactions.Commands.AddCorrection
{
    /// <summary>
    /// Returns the <em>corrected</em> transaction, not the correction itself: corrections are never rendered as
    /// standalone rows, so the parent (with its refreshed NetAmount and the new correction embedded) is what the
    /// client needs to put back on screen.
    /// </summary>
    public record AddCorrectionCommand(
        int TransactionId,
        decimal Amount,
        DateOnly OccurredOn,
        string? Note,
        int UserProfileId) : ICommand<TransactionDto>;
}
