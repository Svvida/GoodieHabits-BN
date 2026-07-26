using Domain.Enums;

namespace Application.Finance.Analytics.Dtos
{
    public class BudgetProgressItemDto
    {
        public int BudgetId { get; set; }
        public int? CategoryId { get; set; }          // null => overall budget
        public string? CategoryName { get; set; }
        public BudgetPeriodEnum Period { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public decimal Limit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentUsed { get; set; }
        public bool IsOverBudget { get; set; }
    }
}
