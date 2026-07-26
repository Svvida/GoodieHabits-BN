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
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
