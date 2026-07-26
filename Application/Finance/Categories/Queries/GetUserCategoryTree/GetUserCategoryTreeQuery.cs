using Application.Common.Interfaces;
using Application.Finance.Categories.Dtos;
using Domain.Enums;

namespace Application.Finance.Categories.Queries.GetUserCategoryTree
{
    // Type is an optional filter (income / expense). Null returns both.
    public record GetUserCategoryTreeQuery(int UserProfileId, FinanceTransactionTypeEnum? Type)
        : IQuery<IEnumerable<FinanceCategoryDto>>;
}
