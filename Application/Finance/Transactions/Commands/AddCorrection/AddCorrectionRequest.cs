namespace Application.Finance.Transactions.Commands.AddCorrection
{
    /// <summary>
    /// Money coming back against an existing transaction (refund, payback, reimbursement).
    /// Type and category are inherited from the corrected transaction and are deliberately not accepted here.
    /// </summary>
    public record AddCorrectionRequest(
        decimal Amount,
        DateOnly OccurredOn,
        string? Note);
}
