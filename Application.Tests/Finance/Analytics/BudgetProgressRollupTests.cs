using Application.Finance.Analytics.Queries.GetBudgetProgress;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Analytics
{
    /// <summary>
    /// Phase 12.0 regression: a budget on a main category has to see the spending filed under its
    /// sub-categories. Before the fix this matched the category id exactly, so a budget on a parent read as
    /// permanently unspent — which is most budgets, given the seeded taxonomy is 6 mains to 47 subs — and made
    /// the Statistics screen disagree with the Dashboard, which rolls up client-side.
    /// </summary>
    public class BudgetProgressRollupTests : TestBase<GetBudgetProgressQueryHandler>
    {
        private static readonly DateOnly SpentOn = new(2026, 1, 15);

        private readonly GetBudgetProgressQueryHandler _budgetProgress;

        public BudgetProgressRollupTests()
        {
            _budgetProgress = new GetBudgetProgressQueryHandler(_unitOfWork);
        }

        [Fact]
        public async Task ParentBudget_ShouldCountSubCategorySpending()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            AddBudget(profile.Id, main.Id, 1000m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result.Should().ContainSingle();
            result[0].Spent.Should().Be(100m);
            result[0].Remaining.Should().Be(900m);
        }

        [Fact]
        public async Task ParentBudget_ShouldCountItsOwnSpendingAndItsSubsTogether()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            await AddExpenseAsync(profile.Id, main.Id, 60m);
            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            AddBudget(profile.Id, main.Id, 1000m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result[0].Spent.Should().Be(160m);
        }

        [Fact]
        public async Task SubCategoryBudget_ShouldNotCountItsParentsSpending()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            await AddExpenseAsync(profile.Id, main.Id, 60m);
            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            AddBudget(profile.Id, sub.Id, 500m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            // Roll-up goes down the tree, never up.
            result[0].Spent.Should().Be(100m);
        }

        [Fact]
        public async Task ParentBudget_ShouldNotCountAnUnrelatedMainsSpending()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            var unrelated = FinanceCategory.CreateMain(profile.Id, "Transport", FinanceTransactionTypeEnum.Expense);
            unrelated.Id = 9003;
            _context.FinanceCategories.Add(unrelated);
            await _context.SaveChangesAsync();

            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            await AddExpenseAsync(profile.Id, unrelated.Id, 900m);
            AddBudget(profile.Id, main.Id, 1000m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result[0].Spent.Should().Be(100m);
            result[0].IsOverBudget.Should().BeFalse();
        }

        [Fact]
        public async Task BudgetsOnAParentAndItsSub_ShouldBothCountThatSubsSpending()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            AddBudget(profile.Id, main.Id, 1000m);
            AddBudget(profile.Id, sub.Id, 500m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            // Deliberate nested-envelope behaviour: the sub's spending counts toward both budgets.
            result.Should().HaveCount(2);
            result.Should().OnlyContain(item => item.Spent == 100m);
        }

        [Fact]
        public async Task OverallBudget_ShouldStillCountEverything()
        {
            var (profile, main, sub) = await ArrangeTreeAsync();

            await AddExpenseAsync(profile.Id, sub.Id, 100m);
            await AddExpenseAsync(profile.Id, main.Id, 60m);
            await AddExpenseAsync(profile.Id, null, 40m);
            AddBudget(profile.Id, null, 1000m);
            await _context.SaveChangesAsync();

            var result = (await _budgetProgress.Handle(new GetBudgetProgressQuery(profile.Id, 2026, 1), CancellationToken.None)).ToList();

            result[0].Spent.Should().Be(200m);
        }

        private async Task<(UserProfile Profile, FinanceCategory Main, FinanceCategory Sub)> ArrangeTreeAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            var profile = account.Profile;

            var main = FinanceCategory.CreateMain(profile.Id, "Mieszkanie", FinanceTransactionTypeEnum.Expense);
            main.Id = 9001;
            _context.FinanceCategories.Add(main);
            await _context.SaveChangesAsync();

            var sub = FinanceCategory.CreateSub(profile.Id, main, "Prad");
            sub.Id = 9002;
            _context.FinanceCategories.Add(sub);
            await _context.SaveChangesAsync();

            return (profile, main, sub);
        }

        private async Task AddExpenseAsync(int userProfileId, int? categoryId, decimal amount)
        {
            _context.FinanceTransactions.Add(
                FinanceTransaction.Create(userProfileId, FinanceTransactionTypeEnum.Expense, amount, SpentOn, categoryId));
            await _context.SaveChangesAsync();
        }

        private void AddBudget(int userProfileId, int? categoryId, decimal limit) =>
            _context.Budgets.Add(Budget.Create(userProfileId, categoryId, BudgetPeriodEnum.Monthly, 2026, 1, limit));
    }
}
