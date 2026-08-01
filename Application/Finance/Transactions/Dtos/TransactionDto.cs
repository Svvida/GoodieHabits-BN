using Domain.Enums;

namespace Application.Finance.Transactions.Dtos
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public FinanceTransactionTypeEnum Type { get; set; }
        public decimal Amount { get; set; }
        public int? CategoryId { get; set; }
        public DateOnly OccurredOn { get; set; }
        public string? Note { get; set; }

        /// <summary>
        /// Whether the money has moved yet. Descriptive only — an unpaid transaction is counted in every
        /// aggregate exactly as if it were paid, including the opening balance. Always <c>true</c> on a
        /// correction. Rows created from a recurring template start unpaid when they are expenses.
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>Set when this row is a correction of another transaction; null for ordinary transactions.</summary>
        public int? CorrectsTransactionId { get; set; }

        /// <summary>How much of <see cref="Amount"/> has come back via corrections. Always 0 on a correction.</summary>
        public decimal CorrectedAmount { get; set; }

        /// <summary><c>Amount - CorrectedAmount</c> — the figure every analytics endpoint already reports.</summary>
        public decimal NetAmount { get; set; }

        /// <summary>Corrections raised against this transaction, regardless of their own date. Empty on a correction.</summary>
        public List<TransactionDto> Corrections { get; set; } = [];

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
