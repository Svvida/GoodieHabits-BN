namespace Application.Finance.Analytics.Dtos
{
    /// <summary>One slice of a category breakdown. ParentCategoryId lets the client roll sub-categories up to their main.</summary>
    public class CategoryBreakdownItemDto
    {
        public int? CategoryId { get; set; }         // null => uncategorized
        public string? CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }      // share of the type's total, 0..100
    }
}
