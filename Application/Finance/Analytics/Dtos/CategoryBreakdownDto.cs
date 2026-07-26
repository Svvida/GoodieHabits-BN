using Domain.Enums;

namespace Application.Finance.Analytics.Dtos
{
    public class CategoryBreakdownDto
    {
        public FinanceTransactionTypeEnum Type { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }               // null => whole year
        public string Currency { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<CategoryBreakdownItemDto> Items { get; set; } = [];
    }
}
