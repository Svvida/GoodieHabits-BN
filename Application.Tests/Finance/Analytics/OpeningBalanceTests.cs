using Application.Finance.Analytics.Queries.GetMonthlySummary;
using Application.Finance.Transactions.Commands.AddCorrection;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Analytics
{
    /// <summary>
    /// Phase 12.2 wiring: the grouped query plus the fold, end to end through the monthly summary.
    /// </summary>
    public class OpeningBalanceTests : TestBase<GetMonthlySummaryQueryHandler>
    {
        private readonly GetMonthlySummaryQueryHandler _monthlySummary;
        private readonly AddCorrectionCommandHandler _addCorrection;

        public OpeningBalanceTests()
        {
            _monthlySummary = new GetMonthlySummaryQueryHandler(_unitOfWork);
            _addCorrection = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task OpeningBalance_ShouldBeZero_ForTheFirstMonth()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 5));

            var january = await SummaryAsync(profile.Id, 2026, 1);

            january.OpeningBalance.Should().Be(0m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldCarryTheLeftoverForward()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 5));
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, new DateOnly(2026, 1, 20));

            var february = await SummaryAsync(profile.Id, 2026, 2);

            february.OpeningBalance.Should().Be(600m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldNetCorrectionsIntoTheParentsMonth()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 5));
            var dinner = await AddAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, new DateOnly(2026, 1, 15));

            // Paid back in February, but it belongs to January's cost either way.
            await _addCorrection.Handle(
                new AddCorrectionCommand(dinner.Id, 300m, new DateOnly(2026, 2, 3), "paid back", profile.Id),
                CancellationToken.None);

            var february = await SummaryAsync(profile.Id, 2026, 2);
            var march = await SummaryAsync(profile.Id, 2026, 3);

            february.OpeningBalance.Should().Be(900m);
            // The correction must not surface again as February activity.
            march.OpeningBalance.Should().Be(900m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldGoNegative_WhenTheUserOverspent()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 100m, new DateOnly(2026, 1, 5));
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 600m, new DateOnly(2026, 1, 20));

            var february = await SummaryAsync(profile.Id, 2026, 2);

            february.OpeningBalance.Should().Be(-500m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldCountUnpaidExpenses()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 5));
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, new DateOnly(2026, 1, 20), isPaid: false);

            // An unpaid bill is money already spoken for — same treatment as a paid one.
            (await SummaryAsync(profile.Id, 2026, 2)).OpeningBalance.Should().Be(600m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldNotSeeAnotherUsersHistory()
        {
            var profile = await ArrangeProfileAsync();
            var other = (await AddAccountAsync("other@test.com", "pass", "other")).Profile;

            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 1000m, new DateOnly(2026, 1, 5));
            await AddAsync(other.Id, FinanceTransactionTypeEnum.Income, 9999m, new DateOnly(2026, 1, 5));

            (await SummaryAsync(profile.Id, 2026, 2)).OpeningBalance.Should().Be(1000m);
        }

        [Fact]
        public async Task OpeningBalance_ShouldSurviveAGapAndCrossYears()
        {
            var profile = await ArrangeProfileAsync();
            await AddAsync(profile.Id, FinanceTransactionTypeEnum.Income, 900m, new DateOnly(2025, 12, 10));

            (await SummaryAsync(profile.Id, 2026, 4)).OpeningBalance.Should().Be(900m);
        }

        private async Task<UserProfile> ArrangeProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<FinanceTransaction> AddAsync(
            int userProfileId, FinanceTransactionTypeEnum type, decimal amount, DateOnly occurredOn, bool isPaid = true)
        {
            var transaction = FinanceTransaction.Create(userProfileId, type, amount, occurredOn, null, null, isPaid);
            _context.FinanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        private async Task<Application.Finance.Analytics.Dtos.MonthlySummaryDto> SummaryAsync(int userProfileId, int year, int month) =>
            await _monthlySummary.Handle(new GetMonthlySummaryQuery(userProfileId, year, month), CancellationToken.None);
    }
}
