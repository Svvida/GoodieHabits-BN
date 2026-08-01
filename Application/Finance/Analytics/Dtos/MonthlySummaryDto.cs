namespace Application.Finance.Analytics.Dtos
{
    public class MonthlySummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Net { get; set; }

        /// <summary>
        /// Money carried into this month from every earlier one (income minus expense, chained from the user's
        /// first month, whose opening balance is 0). Signed — an overspent history carries a negative forward
        /// rather than being clamped to zero, which would erase the shortfall from every later month. Unpaid
        /// transactions reduce it like any other, since they count as if already paid.
        /// </summary>
        public decimal OpeningBalance { get; set; }
        public List<CategoryBreakdownItemDto> ExpenseByCategory { get; set; } = [];
        public List<CategoryBreakdownItemDto> IncomeByCategory { get; set; } = [];
    }
}
