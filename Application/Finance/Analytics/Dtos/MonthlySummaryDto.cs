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
        public List<CategoryBreakdownItemDto> ExpenseByCategory { get; set; } = [];
        public List<CategoryBreakdownItemDto> IncomeByCategory { get; set; } = [];
    }
}
