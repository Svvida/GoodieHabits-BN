using Application.Finance.Analytics.Queries.GetBudgetProgress;
using Application.Finance.Analytics.Queries.GetCategoryBreakdown;
using Application.Finance.Analytics.Queries.GetMonthlySummary;
using Application.Finance.Transactions.Commands.AddCorrection;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Analytics
{
    /// <summary>
    /// The point of materializing the netting on the parent: a correction dated in a later month still reduces
    /// the month the money was actually spent in, and never shows up as activity of its own.
    /// </summary>
    public class CorrectedAnalyticsTests : TestBase<GetMonthlySummaryQueryHandler>
    {
        private static readonly DateOnly Dinner = new(2026, 1, 15);
        private static readonly DateOnly PaidBack = new(2026, 2, 3);

        private readonly AddCorrectionCommandHandler _addCorrection;
        private readonly GetMonthlySummaryQueryHandler _monthlySummary;
        private readonly GetCategoryBreakdownQueryHandler _breakdown;
        private readonly GetBudgetProgressQueryHandler _budgetProgress;

        public CorrectedAnalyticsTests()
        {
            _addCorrection = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
            _monthlySummary = new GetMonthlySummaryQueryHandler(_unitOfWork);
            _breakdown = new GetCategoryBreakdownQueryHandler(_unitOfWork);
            _budgetProgress = new GetBudgetProgressQueryHandler(_unitOfWork);
        }

        [Fact]
        public async Task MonthlySummary_ShouldNetTheParentsMonth_AndLeaveTheCorrectionsOwnMonthUntouched()
        {
            var (profile, _) = await ArrangeCorrectedDinnerAsync();

            var january = await _monthlySummary.Handle(new GetMonthlySummaryQuery(profile.Id, 2026, 1), CancellationToken.None);
            var february = await _monthlySummary.Handle(new GetMonthlySummaryQuery(profile.Id, 2026, 2), CancellationToken.None);

            january.TotalExpense.Should().Be(100m);
            january.Net.Should().Be(-100m);

            // The February payback must not surface as income, nor as an expense of its own.
            february.TotalExpense.Should().Be(0m);
            february.TotalIncome.Should().Be(0m);
        }

        [Fact]
        public async Task CategoryBreakdown_ShouldReportTheNetCategoryFigure()
        {
            var (profile, category) = await ArrangeCorrectedDinnerAsync();

            var result = await _breakdown.Handle(
                new GetCategoryBreakdownQuery(profile.Id, FinanceTransactionTypeEnum.Expense, BudgetPeriodEnum.Monthly, 2026, 1),
                CancellationToken.None);

            result.Total.Should().Be(100m);
            result.Items.Should().ContainSingle();
            result.Items[0].CategoryId.Should().Be(category.Id);
            result.Items[0].Amount.Should().Be(100m);
        }

        [Fact]
        public async Task BudgetProgress_ShouldNetSpending_DespiteItsExpenseOnlyFilter()
        {
            var (profile, category) = await ArrangeCorrectedDinnerAsync();

            _context.Budgets.Add(Budget.Create(profile.Id, category.Id, BudgetPeriodEnum.Monthly, 2026, 1, 200m));
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result.Should().ContainSingle();
            result[0].Spent.Should().Be(100m);
            result[0].Remaining.Should().Be(100m);
            result[0].IsOverBudget.Should().BeFalse();
        }

        [Fact]
        public async Task BudgetProgress_ShouldNotBeFooled_WhenTheGrossAmountWouldBlowTheBudget()
        {
            var (profile, category) = await ArrangeCorrectedDinnerAsync();

            // 400 gross would be over a 350 limit; 100 net is not.
            _context.Budgets.Add(Budget.Create(profile.Id, category.Id, BudgetPeriodEnum.Monthly, 2026, 1, 350m));
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result[0].IsOverBudget.Should().BeFalse();
        }

        private async Task<(UserProfile Profile, FinanceCategory Category)> ArrangeCorrectedDinnerAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            var profile = account.Profile;

            var category = FinanceCategory.CreateMain(profile.Id, "Food", FinanceTransactionTypeEnum.Expense);
            category.Id = 9001;
            _context.FinanceCategories.Add(category);

            var dinner = FinanceTransaction.Create(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, category.Id, "dinner");
            _context.FinanceTransactions.Add(dinner);
            await _context.SaveChangesAsync();

            await _addCorrection.Handle(
                new AddCorrectionCommand(dinner.Id, 300m, PaidBack, "paid back", profile.Id), CancellationToken.None);

            return (profile, category);
        }
    }
}
