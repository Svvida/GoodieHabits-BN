using Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction;
using Application.Finance.RecurringTransactions.Commands.DeleteRecurringTransaction;
using Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction;
using Application.Finance.RecurringTransactions.Queries.GetRecurringTransactions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Finance.RecurringTransactions
{
    public class RecurringTransactionCrudTests : TestBase<CreateRecurringTransactionCommandHandler>
    {
        private readonly CreateRecurringTransactionCommandHandler _create;
        private readonly UpdateRecurringTransactionCommandHandler _update;
        private readonly DeleteRecurringTransactionCommandHandler _delete;
        private readonly GetRecurringTransactionsQueryHandler _get;

        public RecurringTransactionCrudTests()
        {
            _create = new CreateRecurringTransactionCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _update = new UpdateRecurringTransactionCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _delete = new DeleteRecurringTransactionCommandHandler(_unitOfWork);
            _get = new GetRecurringTransactionsQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Create_ShouldStoreTheTemplateAsActive()
        {
            var profile = await ArrangeProfileAsync();

            var result = await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Expense, 40m, 10, null, "Netflix", profile.Id),
                CancellationToken.None);

            result.Amount.Should().Be(40m);
            result.DayOfMonth.Should().Be(10);
            result.IsActive.Should().BeTrue();
            result.Note.Should().Be("Netflix");
        }

        [Fact]
        public async Task Create_ShouldStampTheWatermarkToTheCurrentMonth()
        {
            var profile = await ArrangeProfileAsync();

            await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Expense, 40m, 10, null, null, profile.Id),
                CancellationToken.None);

            var today = DateOnly.FromDateTime(_fixedTestInstant.ToDateTimeUtc());
            var stored = await _context.RecurringTransactions.SingleAsync();

            stored.LastMaterializedOn.Should().Be(new DateOnly(today.Year, today.Month, 1));
        }

        [Fact]
        public async Task Create_ShouldRejectACategoryOfTheWrongType()
        {
            var profile = await ArrangeProfileAsync();
            var expenseCategory = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);

            var act = async () => await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Income, 40m, 10, expenseCategory.Id, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Create_ShouldAcceptASubCategory()
        {
            var profile = await ArrangeProfileAsync();
            var main = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            var sub = await AddSubCategoryAsync(profile.Id, main, "Rent", 9002);

            var result = await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Expense, 40m, 10, sub.Id, null, profile.Id),
                CancellationToken.None);

            result.CategoryId.Should().Be(sub.Id);
        }

        [Fact]
        public async Task Get_ShouldReturnOnlyTheUsersOwnTemplates()
        {
            var profile = await ArrangeProfileAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;

            await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Expense, 40m, 10, null, "mine", profile.Id),
                CancellationToken.None);
            await _create.Handle(
                new CreateRecurringTransactionCommand(FinanceTransactionTypeEnum.Expense, 99m, 3, null, "theirs", other.Id),
                CancellationToken.None);

            var result = (await _get.Handle(new GetRecurringTransactionsQuery(profile.Id), CancellationToken.None)).ToList();

            result.Should().ContainSingle().Which.Note.Should().Be("mine");
        }

        [Fact]
        public async Task Update_ShouldApplyOnlyTheFieldsSent()
        {
            var (profile, template) = await ArrangeTemplateAsync();

            var result = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, 55m, null, null, null, null, false, profile.Id),
                CancellationToken.None);

            result.Amount.Should().Be(55m);
            result.DayOfMonth.Should().Be(10);        // untouched
            result.Note.Should().Be("Netflix");       // untouched
            result.IsActive.Should().BeTrue();        // untouched
        }

        [Fact]
        public async Task Update_ShouldPauseAndResume()
        {
            var (profile, template) = await ArrangeTemplateAsync();

            var paused = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, false, null, false, profile.Id),
                CancellationToken.None);
            paused.IsActive.Should().BeFalse();

            var resumed = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, true, null, false, profile.Id),
                CancellationToken.None);
            resumed.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Update_ShouldReassignTheCategory()
        {
            var (profile, template) = await ArrangeTemplateAsync();
            var main = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            var sub = await AddSubCategoryAsync(profile.Id, main, "Rent", 9002);

            var result = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, null, sub.Id, true, profile.Id),
                CancellationToken.None);

            result.CategoryId.Should().Be(sub.Id);
        }

        [Fact]
        public async Task Update_ShouldClearTheCategory_WhenSentAsNull()
        {
            var (profile, template) = await ArrangeTemplateAsync();
            var main = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            template.UpdateCategory(main.Id);
            await _context.SaveChangesAsync();

            var result = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, null, null, true, profile.Id),
                CancellationToken.None);

            result.CategoryId.Should().BeNull();
        }

        [Fact]
        public async Task Update_ShouldLeaveTheCategoryAlone_WhenTheFieldWasNotSent()
        {
            var (profile, template) = await ArrangeTemplateAsync();
            var main = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            template.UpdateCategory(main.Id);
            await _context.SaveChangesAsync();

            // Same null CategoryId as the test above, but flagged as absent from the request body.
            var result = await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, 55m, null, null, null, null, false, profile.Id),
                CancellationToken.None);

            result.CategoryId.Should().Be(main.Id);
        }

        [Fact]
        public async Task Update_ShouldRejectACategoryOfTheWrongType()
        {
            var (profile, template) = await ArrangeTemplateAsync();   // an expense template
            var income = FinanceCategory.CreateMain(profile.Id, "Salary", FinanceTransactionTypeEnum.Income);
            income.Id = 9101;
            _context.FinanceCategories.Add(income);
            await _context.SaveChangesAsync();

            var act = async () => await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, null, income.Id, true, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Update_ShouldNotRecategorizeAlreadyMaterializedTransactions()
        {
            var (profile, template) = await ArrangeTemplateAsync();
            var main = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            var sub = await AddSubCategoryAsync(profile.Id, main, "Rent", 9002);

            template.UpdateCategory(main.Id);
            var materialized = FinanceTransaction.CreateRecurring(template, new DateOnly(2026, 5, 10));
            _context.FinanceTransactions.Add(materialized);
            await _context.SaveChangesAsync();

            await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, null, null, null, null, sub.Id, true, profile.Id),
                CancellationToken.None);

            // Forward-only: the row already generated keeps the category it was created with.
            var stored = await _context.FinanceTransactions.SingleAsync();
            stored.CategoryId.Should().Be(main.Id);
        }

        [Fact]
        public async Task Update_ShouldNotTouchAnotherUsersTemplate()
        {
            var (_, template) = await ArrangeTemplateAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;

            var act = async () => await _update.Handle(
                new UpdateRecurringTransactionCommand(template.Id, 1m, null, null, null, null, false, other.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Delete_ShouldRemoveTheTemplate()
        {
            var (profile, template) = await ArrangeTemplateAsync();

            await _delete.Handle(new DeleteRecurringTransactionCommand(template.Id, profile.Id), CancellationToken.None);

            (await _context.RecurringTransactions.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Delete_ShouldKeepAlreadyMaterializedTransactions()
        {
            var (profile, template) = await ArrangeTemplateAsync();

            var materialized = FinanceTransaction.CreateRecurring(template, new DateOnly(2026, 5, 10));
            _context.FinanceTransactions.Add(materialized);
            await _context.SaveChangesAsync();

            await _delete.Handle(new DeleteRecurringTransactionCommand(template.Id, profile.Id), CancellationToken.None);

            // The row is the user's own record: it survives, merely unlinked from the template.
            var survivor = await _context.FinanceTransactions.SingleAsync();
            survivor.Amount.Should().Be(40m);
            survivor.RecurringTransactionId.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldNotTouchAnotherUsersTemplate()
        {
            var (_, template) = await ArrangeTemplateAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;

            var act = async () => await _delete.Handle(
                new DeleteRecurringTransactionCommand(template.Id, other.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        private async Task<UserProfile> ArrangeProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<FinanceCategory> AddCategoryAsync(int userProfileId, FinanceTransactionTypeEnum type)
        {
            var category = FinanceCategory.CreateMain(userProfileId, "Home", type);
            category.Id = 9001;
            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        private async Task<FinanceCategory> AddSubCategoryAsync(int userProfileId, FinanceCategory parent, string name, int id)
        {
            var sub = FinanceCategory.CreateSub(userProfileId, parent, name);
            sub.Id = id;
            _context.FinanceCategories.Add(sub);
            await _context.SaveChangesAsync();
            return sub;
        }

        private async Task<(UserProfile Profile, RecurringTransaction Template)> ArrangeTemplateAsync()
        {
            var profile = await ArrangeProfileAsync();

            var today = DateOnly.FromDateTime(_fixedTestInstant.ToDateTimeUtc());
            var template = RecurringTransaction.Create(
                profile.Id, FinanceTransactionTypeEnum.Expense, 40m, 10, today, null, "Netflix");
            _context.RecurringTransactions.Add(template);
            await _context.SaveChangesAsync();

            return (profile, template);
        }
    }
}
