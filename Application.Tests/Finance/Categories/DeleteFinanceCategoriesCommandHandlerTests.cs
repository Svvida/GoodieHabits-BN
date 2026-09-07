using Application.Finance.Categories.Commands.DeleteFinanceCategories;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Categories
{
    public class DeleteFinanceCategoriesCommandHandlerTests : TestBase<DeleteFinanceCategoriesCommandHandler>
    {
        private readonly DeleteFinanceCategoriesCommandHandler _handler;

        public DeleteFinanceCategoriesCommandHandlerTests()
        {
            _handler = new DeleteFinanceCategoriesCommandHandler(_unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenCategoryNotOwned()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new DeleteFinanceCategoriesCommand(new[] { 999_999 }, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenCategoryHasTransactions()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);

            _context.FinanceTransactions.Add(
                FinanceTransaction.Create(profile.Id, FinanceTransactionTypeEnum.Expense, 10m, new DateOnly(2026, 1, 1), main.Id, null));
            await _context.SaveChangesAsync();

            var command = new DeleteFinanceCategoriesCommand(new[] { main.Id }, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenCategoryIsUsedByARecurringTemplate()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);
            var sub = await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Rent"), 9002);

            // No transaction has been materialized yet — the template alone holds the (Restrict) FK.
            _context.RecurringTransactions.Add(RecurringTransaction.Create(
                profile.Id, FinanceTransactionTypeEnum.Expense, 1200m, 5, new DateOnly(2026, 1, 1), sub.Id));
            await _context.SaveChangesAsync();

            var command = new DeleteFinanceCategoriesCommand(new[] { main.Id, sub.Id }, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenCategoryHasABudget()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);

            _context.Budgets.Add(Budget.Create(profile.Id, main.Id, BudgetPeriodEnum.Monthly, 2026, 1, 1500m));
            await _context.SaveChangesAsync();

            var command = new DeleteFinanceCategoriesCommand(new[] { main.Id }, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenMainHasExcludedChild()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);
            await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Rent"), 9002);

            // Only the main is requested; its sub-category is excluded.
            var command = new DeleteFinanceCategoriesCommand(new[] { main.Id }, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldDeleteMainAndChild_WhenBothIncluded()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var main = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense), 9001);
            var sub = await AddCategoryAsync(FinanceCategory.CreateSub(profile.Id, main, "Rent"), 9002);

            var command = new DeleteFinanceCategoriesCommand(new[] { main.Id, sub.Id }, profile.Id);

            await _handler.Handle(command, CancellationToken.None);

            (await _context.FinanceCategories.FindAsync(main.Id)).Should().BeNull();
            (await _context.FinanceCategories.FindAsync(sub.Id)).Should().BeNull();
        }

        private async Task ResetFinanceAsync()
        {
            _context.FinanceTransactions.RemoveRange(_context.FinanceTransactions);
            _context.RecurringTransactions.RemoveRange(_context.RecurringTransactions);
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
