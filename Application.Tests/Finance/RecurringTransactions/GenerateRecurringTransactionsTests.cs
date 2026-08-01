using Application.Finance.RecurringTransactions.Commands.GenerateRecurringTransactions;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;

namespace Application.Tests.Finance.RecurringTransactions
{
    /// <summary>
    /// Phase 12.3 materialization. The catch-up is correct whenever it fires and harmless if it fires twice —
    /// these tests are what hold that property, especially the deleted-row case the watermark exists for.
    /// </summary>
    public class GenerateRecurringTransactionsTests : TestBase<GenerateRecurringTransactionsCommandHandler>
    {
        private readonly GenerateRecurringTransactionsCommandHandler _generate;

        public GenerateRecurringTransactionsTests()
        {
            _generate = new GenerateRecurringTransactionsCommandHandler(_unitOfWork, _clockMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Generate_ShouldDoNothing_InTheTemplatesCreationMonth()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today());

            var generated = await RunAsync();

            generated.Should().Be(0);
            (await _context.FinanceTransactions.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Generate_ShouldCreateOneRowForTheMonthAfterCreation()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));

            var generated = await RunAsync();

            generated.Should().Be(1);
            var transaction = await _context.FinanceTransactions.SingleAsync();
            transaction.Amount.Should().Be(40m);
            transaction.OccurredOn.Should().Be(new DateOnly(Today().Year, Today().Month, 10));
        }

        [Fact]
        public async Task Generate_ShouldBeIdempotent_WhenRunTwice()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));

            (await RunAsync()).Should().Be(1);
            (await RunAsync()).Should().Be(0);

            (await _context.FinanceTransactions.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Generate_ShouldBackfillEveryMissedMonth()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-3));

            var generated = await RunAsync();

            generated.Should().Be(3);
            (await _context.FinanceTransactions.CountAsync()).Should().Be(3);
        }

        [Fact]
        public async Task Generate_ShouldNotResurrectARowTheUserDeleted()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));

            await RunAsync();
            var materialized = await _context.FinanceTransactions.SingleAsync();
            _context.FinanceTransactions.Remove(materialized);
            await _context.SaveChangesAsync();

            // This is the whole reason for the watermark: pure existence-checking would recreate it here,
            // which reads as a bug to the user who just deleted it.
            (await RunAsync()).Should().Be(0);
            (await _context.FinanceTransactions.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Generate_ShouldMarkExpensesUnpaid()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));

            await RunAsync();

            (await _context.FinanceTransactions.SingleAsync()).IsPaid.Should().BeFalse();
        }

        [Fact]
        public async Task Generate_ShouldMarkIncomePaid()
        {
            var profile = await ArrangeProfileAsync();
            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1), type: FinanceTransactionTypeEnum.Income);

            await RunAsync();

            // "Unpaid" has no meaning for money coming in.
            (await _context.FinanceTransactions.SingleAsync()).IsPaid.Should().BeTrue();
        }

        [Fact]
        public async Task Generate_ShouldStampProvenance()
        {
            var profile = await ArrangeProfileAsync();
            var template = await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));

            await RunAsync();

            (await _context.FinanceTransactions.SingleAsync()).RecurringTransactionId.Should().Be(template.Id);
        }

        [Fact]
        public async Task Generate_ShouldSkipInactiveTemplates()
        {
            var profile = await ArrangeProfileAsync();
            var template = await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-2));
            template.SetActive(false, Today().AddMonths(-2));
            await _context.SaveChangesAsync();

            (await RunAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Generate_ShouldCarryTheCategoryAndNote()
        {
            var profile = await ArrangeProfileAsync();

            var category = FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense);
            category.Id = 9001;
            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();

            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1), categoryId: category.Id);

            await RunAsync();

            var transaction = await _context.FinanceTransactions.SingleAsync();
            transaction.CategoryId.Should().Be(category.Id);
            transaction.Note.Should().Be("Netflix");
        }

        [Fact]
        public async Task Generate_ShouldHandleTemplatesForSeveralUsersInOneSweep()
        {
            var profile = await ArrangeProfileAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;

            await AddTemplateAsync(profile.Id, createdOn: Today().AddMonths(-1));
            await AddTemplateAsync(other.Id, createdOn: Today().AddMonths(-1));

            (await RunAsync()).Should().Be(2);
        }

        private DateOnly Today() => DateOnly.FromDateTime(_fixedTestInstant.ToDateTimeUtc());

        private Task<int> RunAsync() =>
            _generate.Handle(new GenerateRecurringTransactionsCommand(), CancellationToken.None);

        private async Task<UserProfile> ArrangeProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<RecurringTransaction> AddTemplateAsync(
            int userProfileId,
            DateOnly createdOn,
            FinanceTransactionTypeEnum type = FinanceTransactionTypeEnum.Expense,
            int? categoryId = null)
        {
            var template = RecurringTransaction.Create(userProfileId, type, 40m, 10, createdOn, categoryId, "Netflix");
            _context.RecurringTransactions.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }
    }
}
