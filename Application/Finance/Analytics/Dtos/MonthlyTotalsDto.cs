namespace Application.Finance.Analytics.Dtos
{
    public class MonthlyTotalsDto
    {
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Net { get; set; }
    }
}
