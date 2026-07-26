using Domain.Enums;

namespace Application.Finance.Categories.Dtos
{
    public class FinanceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public FinanceTransactionTypeEnum Type { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsSystem { get; set; }
        public bool IsSavings { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<FinanceCategoryDto> SubCategories { get; set; } = [];
    }
}
