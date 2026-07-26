using Application.Common.Interfaces;

namespace Application.Finance.Categories.Commands.DeleteFinanceCategories
{
    public record DeleteFinanceCategoriesCommand(IReadOnlyList<int> CategoryIds, int UserProfileId) : ICommand;
}
