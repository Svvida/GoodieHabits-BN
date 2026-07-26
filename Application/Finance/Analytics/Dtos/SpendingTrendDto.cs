namespace Application.Finance.Analytics.Dtos
{
    public class SpendingTrendPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net { get; set; }
    }

    public class SpendingTrendDto
    {
        public string Currency { get; set; } = string.Empty;
        public List<SpendingTrendPointDto> Points { get; set; } = [];   // chronological
    }
}
