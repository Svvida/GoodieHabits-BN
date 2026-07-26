using Application.Finance.Transactions.Commands.CreateTransaction;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Transactions
{
    public class CreateTransactionCommandHandlerTests : TestBase<CreateTransactionCommandHandler>
    {
        private readonly CreateTransactionCommandHandler _handler;

        public CreateTransactionCommandHandlerTests()
        {
            _handler = new CreateTransactionCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldCreateTransaction_WhenCategoryTypeMatches()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var category = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Food", FinanceTransactionTypeEnum.Expense), 9001);

            var command = new CreateTransactionCommand(FinanceTransactionTypeEnum.Expense, 25m, new DateOnly(2026, 1, 15), category.Id, "lunch", profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Amount.Should().Be(25m);
            result.CategoryId.Should().Be(category.Id);
            _context.FinanceTransactions.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenCategoryTypeMismatches()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var expenseCategory = await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Food", FinanceTransactionTypeEnum.Expense), 9001);

            var command = new CreateTransactionCommand(FinanceTransactionTypeEnum.Income, 25m, new DateOnly(2026, 1, 15), expenseCategory.Id, null, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldCreateUncategorizedTransaction()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new CreateTransactionCommand(FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 1), null, "salary", profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.CategoryId.Should().BeNull();
            result.Type.Should().Be(FinanceTransactionTypeEnum.Income);
        }

        private async Task ResetFinanceAsync()
        {
            _context.FinanceTransactions.RemoveRange(_context.FinanceTransactions);
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
