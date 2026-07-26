using Application.Finance.Categories.Commands.UpdateFinanceCategory;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Finance.Categories
{
    public class UpdateFinanceCategoryCommandHandlerTests : TestBase<UpdateFinanceCategoryCommandHandler>
    {
        private readonly UpdateFinanceCategoryCommandHandler _handler;

        public UpdateFinanceCategoryCommandHandlerTests()
        {
            _handler = new UpdateFinanceCategoryCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldFlagMainAsSavings_AndCascadeToSubCategories()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Reserves", FinanceTransactionTypeEnum.Expense), 9001);
            await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Emergency fund"), 9002);
            await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "House deposit"), 9003);

            var command = new UpdateFinanceCategoryCommand(main.Id, "Reserves", null, null, true, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSavings.Should().BeTrue();

            var subs = await _context.FinanceCategories.AsNoTracking()
                .Where(c => c.ParentCategoryId == main.Id)
                .ToListAsync();
            subs.Should().HaveCount(2);
            subs.Should().OnlyContain(c => c.IsSavings);
        }

        [Fact]
        public async Task Handle_ShouldClearSavingsFlag_AndCascadeToSubCategories()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var main = await AddCategoryAsync(
                FinanceCategory.CreateMain(profile.Id, "Reserves", FinanceTransactionTypeEnum.Expense, isSavings: true), 9001);
            await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Emergency fund"), 9002);

            var command = new UpdateFinanceCategoryCommand(main.Id, "Reserves", null, null, false, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSavings.Should().BeFalse();

            var sub = await _context.FinanceCategories.AsNoTracking().SingleAsync(c => c.Id == 9002);
            sub.IsSavings.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldIgnoreIsSavings_OnSubCategory()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);
            var sub = await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Internet"), 9002);

            var command = new UpdateFinanceCategoryCommand(sub.Id, "Internet", null, null, true, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            // The sub inherits from its (non-savings) parent, so the requested value is discarded.
            result.IsSavings.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_ForSystemCategory()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var system = await AddCategoryAsync(FinanceCategory.CreateSystemMain(9001, "Home", FinanceTransactionTypeEnum.Expense));

            var command = new UpdateFinanceCategoryCommand(system.Id, "Renamed", null, null, false, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        private async Task ResetFinanceAsync()
        {
            _context.FinanceTransactions.RemoveRange(_context.FinanceTransactions);
            _context.Budgets.RemoveRange(_context.Budgets);
            _context.FinanceCategories.RemoveRange(_context.FinanceCategories);
            await _context.SaveChangesAsync();
        }

        private async Task<UserProfile> CreateProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<FinanceCategory> AddCategoryAsync(FinanceCategory category, int? id = null)
        {
            if (id.HasValue)
                category.Id = id.Value;
            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}
