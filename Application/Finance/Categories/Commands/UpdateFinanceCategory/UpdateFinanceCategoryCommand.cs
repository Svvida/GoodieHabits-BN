using Application.Common.Interfaces;
using Application.Finance.Categories.Dtos;

namespace Application.Finance.Categories.Commands.UpdateFinanceCategory
{
    public record UpdateFinanceCategoryCommand(
        int CategoryId,
        string Name,
        string? Color,
        string? Icon,
        bool IsSavings,
        int UserProfileId) : ICommand<FinanceCategoryDto>;
}
