using Domain.Enums;

namespace Application.Finance.Budgets.Dtos
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }   // null => overall budget for the period
        public BudgetPeriodEnum Period { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public decimal LimitAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
