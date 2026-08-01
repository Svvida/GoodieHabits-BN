using Domain.Enums;

namespace Application.Finance.RecurringTransactions.Dtos
{
    public class RecurringTransactionDto
    {
        public int Id { get; set; }
        public FinanceTransactionTypeEnum Type { get; set; }
        public int? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }

        /// <summary>1-31; days past the end of a short month clamp to its last day.</summary>
        public int DayOfMonth { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
