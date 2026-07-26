namespace Application.Finance.Categories.Commands.DeleteFinanceCategories
{
    public record DeleteFinanceCategoriesRequest(IReadOnlyList<int> CategoryIds);
}
