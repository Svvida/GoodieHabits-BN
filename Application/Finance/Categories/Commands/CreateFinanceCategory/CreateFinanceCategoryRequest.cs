using Domain.Enums;

namespace Application.Finance.Categories.Commands.CreateFinanceCategory
{
    // When ParentCategoryId is set (creating a sub-category), Type and IsSavings are inherited from
    // the parent and the provided values are ignored.
    public record CreateFinanceCategoryRequest(
        string Name,
        FinanceTransactionTypeEnum Type,
        int? ParentCategoryId,
        string? Color,
        string? Icon,
        bool IsSavings = false);
}
