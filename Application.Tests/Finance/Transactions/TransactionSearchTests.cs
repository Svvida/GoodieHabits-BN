using Application.Finance.Transactions.Commands.AddCorrection;
using Application.Finance.Transactions.Queries.GetTransactions;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Transactions
{
    /// <summary>
    /// The search term covers everything a row displays — its note, its category, and the notes of its
    /// corrections — so "I can see the text, therefore search finds it" holds for the client.
    /// <para>
    /// Every term here matches on exact case on purpose: case sensitivity is left to the database collation
    /// (case-insensitive by default on SQL Server), and EF Core InMemory is case-sensitive, so asserting
    /// otherwise would pin behaviour these tests cannot actually observe.
    /// </para>
    /// </summary>
    public class TransactionSearchTests : TestBase<GetTransactionsQueryHandler>
    {
        private static readonly DateOnly Shop = new(2026, 1, 5);
        private static readonly DateOnly Subscription = new(2026, 2, 10);
        private static readonly DateOnly Uncategorized = new(2026, 3, 1);

        private readonly GetTransactionsQueryHandler _list;
        private readonly AddCorrectionCommandHandler _addCorrection;

        public TransactionSearchTests()
        {
            _list = new GetTransactionsQueryHandler(_unitOfWork, _mapper);
            _addCorrection = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Search_ShouldMatchOnTheNote()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, "Netflix");

            page.Items.Should().ContainSingle().Which.Note.Should().Be("Netflix subscription");
        }

        [Fact]
        public async Task Search_ShouldMatchAnywhereInTheNote()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, "flix");

            page.Items.Should().ContainSingle().Which.Note.Should().Be("Netflix subscription");
        }

        [Fact]
        public async Task Search_ShouldMatchOnTheCategoryName()
        {
            var profile = await ArrangeAsync();

            // "Groceries" appears nowhere in the note — only on the category the row is tagged with.
            var page = await ListAsync(profile.Id, "Groceries");

            page.Items.Should().ContainSingle().Which.Note.Should().Be("weekly shop");
        }

        [Fact]
        public async Task Search_ShouldMatchACorrectionsNote_AndReturnItsParent()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, "refund");

            // The correction is never a row of its own; the parent carrying it is what comes back.
            var row = page.Items.Should().ContainSingle().Subject;
            row.Note.Should().Be("weekly shop");
            row.Corrections.Should().ContainSingle();
        }

        [Fact]
        public async Task Search_ShouldNarrowTheTotalCount()
        {
            var profile = await ArrangeAsync();

            var all = await ListAsync(profile.Id, null);
            var matching = await ListAsync(profile.Id, "Netflix");

            all.TotalCount.Should().Be(3);
            matching.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Search_ShouldReturnNothing_WhenNothingMatches()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, "mortgage");

            page.Items.Should().BeEmpty();
            page.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Search_ShouldComposeWithTheDateRange()
        {
            var profile = await ArrangeAsync();

            // The matching row is dated in February, so a January window has to exclude it.
            var page = await _list.Handle(
                new GetTransactionsQuery(profile.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), null, null, null, "Netflix", 1, 20),
                CancellationToken.None);

            page.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Search_ShouldBeIgnored_WhenBlank()
        {
            var profile = await ArrangeAsync();

            var page = await ListAsync(profile.Id, "   ");

            page.TotalCount.Should().Be(3);
        }

        [Fact]
        public async Task Search_ShouldNotReachAnotherUsersTransactions()
        {
            var profile = await ArrangeAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;
            _context.FinanceTransactions.Add(FinanceTransaction.Create(
                other.Id, FinanceTransactionTypeEnum.Expense, 15m, Subscription, null, "Netflix theirs"));
            await _context.SaveChangesAsync();

            var page = await ListAsync(profile.Id, "Netflix");

            page.Items.Should().ContainSingle().Which.Note.Should().Be("Netflix subscription");
        }

        private Task<Application.Common.Dtos.PagedResult<Application.Finance.Transactions.Dtos.TransactionDto>> ListAsync(
            int userProfileId, string? search) =>
            _list.Handle(
                new GetTransactionsQuery(userProfileId, null, null, null, null, null, search, 1, 20),
                CancellationToken.None);

        private async Task<UserProfile> ArrangeAsync()
        {
            var profile = (await AddAccountAsync("user@test.com", "pass", "user")).Profile;

            var groceries = FinanceCategory.CreateMain(profile.Id, "Groceries", FinanceTransactionTypeEnum.Expense);
            groceries.Id = 9001;
            var fun = FinanceCategory.CreateMain(profile.Id, "Fun", FinanceTransactionTypeEnum.Expense);
            fun.Id = 9002;
            _context.FinanceCategories.AddRange(groceries, fun);

            var shop = FinanceTransaction.Create(
                profile.Id, FinanceTransactionTypeEnum.Expense, 200m, Shop, groceries.Id, "weekly shop");
            _context.FinanceTransactions.AddRange(
                shop,
                FinanceTransaction.Create(profile.Id, FinanceTransactionTypeEnum.Expense, 40m, Subscription, fun.Id, "Netflix subscription"),
                FinanceTransaction.Create(profile.Id, FinanceTransactionTypeEnum.Expense, 12m, Uncategorized, null, null));
            await _context.SaveChangesAsync();

            await _addCorrection.Handle(
                new AddCorrectionCommand(shop.Id, 20m, Shop, "refund for spoiled milk", profile.Id), CancellationToken.None);

            return profile;
        }
    }
}
