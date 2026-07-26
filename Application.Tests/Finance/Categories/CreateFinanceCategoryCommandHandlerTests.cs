using Application.Finance.Categories.Commands.CreateFinanceCategory;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Categories
{
    public class CreateFinanceCategoryCommandHandlerTests : TestBase<CreateFinanceCategoryCommandHandler>
    {
        private readonly CreateFinanceCategoryCommandHandler _handler;

        public CreateFinanceCategoryCommandHandlerTests()
        {
            _handler = new CreateFinanceCategoryCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldCreateMainCategory()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new CreateFinanceCategoryCommand("Travel", FinanceTransactionTypeEnum.Expense, null, "#4E79A7", "plane", false, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Name.Should().Be("Travel");
            result.ParentCategoryId.Should().BeNull();
            result.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
        }

        [Fact]
        public async Task Handle_ShouldCreateSubUnderSystemParent_InheritingType()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var systemMain = await AddCategoryAsync(FinanceCategory.CreateSystemMain(9001, "Home", FinanceTransactionTypeEnum.Expense));

            var command = new CreateFinanceCategoryCommand("Netflix", FinanceTransactionTypeEnum.Income /* ignored */, systemMain.Id, null, null, false, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.ParentCategoryId.Should().Be(systemMain.Id);
            result.Type.Should().Be(FinanceTransactionTypeEnum.Expense); // inherited from parent
        }

        [Fact]
        public async Task Handle_ShouldCreateMainCategory_FlaggedAsSavings()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new CreateFinanceCategoryCommand("Savings", FinanceTransactionTypeEnum.Expense, null, null, null, true, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSavings.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldInheritIsSavingsFromParent_IgnoringRequestedValue()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            var savingsMain = await AddCategoryAsync(
                FinanceCategory.CreateMain(profile.Id, "Savings", FinanceTransactionTypeEnum.Expense, isSavings: true), 9001);

            var command = new CreateFinanceCategoryCommand("Emergency fund", FinanceTransactionTypeEnum.Expense, savingsMain.Id, null, null, false /* ignored */, profile.Id);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSavings.Should().BeTrue(); // inherited from parent
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenDuplicateNameAtSameLevel()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();
            await AddCategoryAsync(FinanceCategory.CreateMain(profile.Id, "Food", FinanceTransactionTypeEnum.Expense), 9001);

            var command = new CreateFinanceCategoryCommand("Food", FinanceTransactionTypeEnum.Expense, null, null, null, false, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenParentDoesNotExist()
        {
            await ResetFinanceAsync();
            var profile = await CreateProfileAsync();

            var command = new CreateFinanceCategoryCommand("Rent", FinanceTransactionTypeEnum.Expense, 999_999, null, null, false, profile.Id);

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
