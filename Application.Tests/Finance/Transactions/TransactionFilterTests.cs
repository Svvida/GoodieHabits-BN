using Application.Common.Dtos;
using Application.Finance.Transactions.Dtos;
using Application.Finance.Transactions.Queries.GetTransactions;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Transactions
{
    /// <summary>
    /// The category filter takes a set, and a main category in that set stands for everything filed under it —
    /// the same reading <c>GetBudgetProgressQueryHandler</c> gives a budget, so "Home" cannot mean one thing on
    /// the dashboard and another in history. Asking for a sub's own id is how you narrow to just that sub.
    /// </summary>
    public class TransactionFilterTests : TestBase<GetTransactionsQueryHandler>
    {
        private const int HomeId = 9001;
        private const int RentId = 9002;
        private const int FunId = 9003;

        private static readonly DateOnly Day = new(2026, 1, 10);

        private readonly GetTransactionsQueryHandler _list;

        public TransactionFilterTests()
        {
            _list = new GetTransactionsQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task CategoryIds_ShouldIncludeSubCategories_WhenAMainIsRequested()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, categoryIds: [HomeId]);

            page.Items.Select(t => t.Note).Should().BeEquivalentTo("home insurance", "january rent");
            page.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task CategoryIds_ShouldNarrowToTheSub_WhenTheSubsOwnIdIsRequested()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, categoryIds: [RentId]);

            page.Items.Should().ContainSingle().Which.Note.Should().Be("january rent");
        }

        [Fact]
        public async Task CategoryIds_ShouldUnionEveryRequestedCategory()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, categoryIds: [RentId, FunId]);

            page.Items.Select(t => t.Note).Should().BeEquivalentTo("january rent", "cinema");
        }

        [Fact]
        public async Task CategoryIds_ShouldBeIgnored_WhenEmpty()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, categoryIds: []);

            page.TotalCount.Should().Be(4);
        }

        [Fact]
        public async Task CategoryIds_ShouldExcludeUncategorizedRows()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, categoryIds: [HomeId, FunId]);

            page.Items.Should().NotContain(t => t.Note == "unfiled");
        }

        [Fact]
        public async Task IsPaid_ShouldReturnOnlyUnpaidRows_WhenFalse()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, isPaid: false);

            page.Items.Select(t => t.Note).Should().BeEquivalentTo("january rent", "unfiled");
            page.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task IsPaid_ShouldReturnOnlyPaidRows_WhenTrue()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, isPaid: true);

            page.Items.Select(t => t.Note).Should().BeEquivalentTo("home insurance", "cinema");
        }

        [Fact]
        public async Task Filters_ShouldCompose()
        {
            var profile = await ArrangeAsync();

            // Everything under Home, but only what is still owed.
            var page = await ListAsync(profile.Id, categoryIds: [HomeId], isPaid: false);

            page.Items.Should().ContainSingle().Which.Note.Should().Be("january rent");
        }

        private Task<PagedResult<TransactionDto>> ListAsync(
            int userProfileId, int[]? categoryIds = null, bool? isPaid = null) =>
            _list.Handle(
                new GetTransactionsQuery(userProfileId, null, null, null, categoryIds, isPaid, null, 1, 20),
                CancellationToken.None);

        private async Task<UserProfile> ArrangeAsync()
        {
            var profile = (await AddAccountAsync("user@test.com", "pass", "user")).Profile;

            var home = FinanceCategory.CreateMain(profile.Id, "Home", FinanceTransactionTypeEnum.Expense);
            home.Id = HomeId;
            var rent = FinanceCategory.CreateSub(profile.Id, home, "Rent");
            rent.Id = RentId;
            var fun = FinanceCategory.CreateMain(profile.Id, "Fun", FinanceTransactionTypeEnum.Expense);
            fun.Id = FunId;
            _context.FinanceCategories.AddRange(home, rent, fun);

            _context.FinanceTransactions.AddRange(
                Transaction(profile.Id, HomeId, "home insurance", isPaid: true),
                Transaction(profile.Id, RentId, "january rent", isPaid: false),
                Transaction(profile.Id, FunId, "cinema", isPaid: true),
                Transaction(profile.Id, null, "unfiled", isPaid: false));
            await _context.SaveChangesAsync();

            return profile;
        }

        private static FinanceTransaction Transaction(int userProfileId, int? categoryId, string note, bool isPaid)
        {
            var transaction = FinanceTransaction.Create(
                userProfileId, FinanceTransactionTypeEnum.Expense, 100m, Day, categoryId, note);
            transaction.MarkPaid(isPaid);
            return transaction;
        }
    }
}
