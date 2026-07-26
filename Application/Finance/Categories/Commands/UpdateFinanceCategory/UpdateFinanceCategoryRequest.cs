namespace Application.Finance.Categories.Commands.UpdateFinanceCategory
{
    // IsSavings only applies to main categories; on a sub-category it is inherited from the parent
    // and the provided value is ignored.
    public record UpdateFinanceCategoryRequest(string Name, string? Color, string? Icon, bool IsSavings = false);
}
