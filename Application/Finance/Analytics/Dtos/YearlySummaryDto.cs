namespace Application.Finance.Analytics.Dtos
{
    public class YearlySummaryDto
    {
        public int Year { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Net { get; set; }
        public List<MonthlyTotalsDto> Months { get; set; } = [];   // 12 entries, Jan..Dec
        public List<CategoryBreakdownItemDto> ExpenseByCategory { get; set; } = [];
        public List<CategoryBreakdownItemDto> IncomeByCategory { get; set; } = [];
    }
}
