using Application.Finance.Budgets.Commands.CreateBudget;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Finance.Budgets
{
    public class CreateBudgetCommandHandlerTests : TestBase<CreateBudgetCommandHandler>
    {
        private readonly CreateBudgetCommandHandler _handler;

        public CreateBudgetCommandHandlerTests()
        {
            _handler = new CreateBudgetCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenDuplicateScopeAndPeriod()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            await _handler.Handle(new CreateBudgetCommand(null, BudgetPeriodEnum.Monthly, 2026, 1, 100m, profile.Id), CancellationToken.None);

            var duplicate = new CreateBudgetCommand(null, BudgetPeriodEnum.Monthly, 2026, 1, 200m, profile.Id);
            var act = () => _handler.Handle(duplicate, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldAllowOverallAndCategoryBudgetsToCoexist()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var category = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);

            await _handler.Handle(new CreateBudgetCommand(null, BudgetPeriodEnum.Monthly, 2027, 1, 1000m, profile.Id), CancellationToken.None);
            await _handler.Handle(new CreateBudgetCommand(category.Id, BudgetPeriodEnum.Monthly, 2027, 1, 400m, profile.Id), CancellationToken.None);

            var count = await _context.Budgets.CountAsync(b => b.UserProfileId == profile.Id && b.Year == 2027 && b.Month == 1);
            count.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenCategoryDoesNotExist()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new CreateBudgetCommand(999_999, BudgetPeriodEnum.Monthly, 2026, 1, 100m, profile.Id);
            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        private async Task ResetFinanceAsync()
        {
            _context.Budgets.RemoveRange(_context.Budgets);
            _context.FinanceCategories.RemoveRange(_context.FinanceCategories);
            await _context.SaveChangesAsync();
        }

        private async Task<UserProfile> CreateProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<FinanceCategory> AddCategoryAsync(FinanceCategory category, int id)
        {
            category.Id = id;
            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}
