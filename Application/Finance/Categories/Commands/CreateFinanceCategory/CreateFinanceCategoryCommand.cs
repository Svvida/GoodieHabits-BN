using Application.Common.Interfaces;
using Application.Finance.Categories.Dtos;
using Domain.Enums;

namespace Application.Finance.Categories.Commands.CreateFinanceCategory
{
    public record CreateFinanceCategoryCommand(
        string Name,
        FinanceTransactionTypeEnum Type,
        int? ParentCategoryId,
        string? Color,
        string? Icon,
        bool IsSavings,
        int UserProfileId) : ICommand<FinanceCategoryDto>;
}
