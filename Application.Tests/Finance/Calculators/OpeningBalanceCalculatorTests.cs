using Domain.Calculators;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Calculators
{
    /// <summary>
    /// Phase 12.2. Pure fold, no DB — this is where the opening-balance edge cases live, precisely because the
    /// InMemory provider would happily pass date-boundary logic that real SQL gets wrong.
    /// </summary>
    public class OpeningBalanceCalculatorTests
    {
        private static MonthlyTotal Income(int year, int month, decimal amount) =>
            new(year, month, FinanceTransactionTypeEnum.Income, amount);

        private static MonthlyTotal Expense(int year, int month, decimal amount) =>
            new(year, month, FinanceTransactionTypeEnum.Expense, amount);

        [Fact]
        public void Calculate_ShouldReturnZero_WhenThereIsNoHistory()
        {
            OpeningBalanceCalculator.Calculate([], 2026, 3).Should().Be(0m);
        }

        [Fact]
        public void Calculate_ShouldReturnZero_ForTheFirstMonthWithData()
        {
            MonthlyTotal[] totals = [Income(2026, 1, 1000m), Expense(2026, 1, 400m)];

            OpeningBalanceCalculator.Calculate(totals, 2026, 1).Should().Be(0m);
        }

        [Fact]
        public void Calculate_ShouldCarryTheLeftoverForward()
        {
            MonthlyTotal[] totals = [Income(2026, 1, 1000m), Expense(2026, 1, 400m)];

            OpeningBalanceCalculator.Calculate(totals, 2026, 2).Should().Be(600m);
        }

        [Fact]
        public void Calculate_ShouldChainAcrossSeveralMonths()
        {
            MonthlyTotal[] totals =
            [
                Income(2026, 1, 1000m), Expense(2026, 1, 400m),   // +600
                Income(2026, 2, 1000m), Expense(2026, 2, 900m),   // +100
            ];

            OpeningBalanceCalculator.Calculate(totals, 2026, 3).Should().Be(700m);
        }

        [Fact]
        public void Calculate_ShouldExcludeTheRequestedMonthItself()
        {
            MonthlyTotal[] totals = [Income(2026, 1, 1000m), Income(2026, 2, 5000m)];

            // February's own income is this month's money, not what was carried into it.
            OpeningBalanceCalculator.Calculate(totals, 2026, 2).Should().Be(1000m);
        }

        [Fact]
        public void Calculate_ShouldSurviveGapMonths()
        {
            MonthlyTotal[] totals = [Income(2026, 1, 1000m), Expense(2026, 5, 200m)];

            // Nothing happened Feb-Apr; the balance simply persists across them.
            OpeningBalanceCalculator.Calculate(totals, 2026, 5).Should().Be(1000m);
            OpeningBalanceCalculator.Calculate(totals, 2026, 6).Should().Be(800m);
        }

        [Fact]
        public void Calculate_ShouldGoNegative_RatherThanClampToZero()
        {
            MonthlyTotal[] totals = [Income(2026, 1, 100m), Expense(2026, 1, 600m)];

            OpeningBalanceCalculator.Calculate(totals, 2026, 2).Should().Be(-500m);
        }

        [Fact]
        public void Calculate_ShouldKeepAShortfallInTheChain()
        {
            MonthlyTotal[] totals =
            [
                Income(2026, 1, 100m), Expense(2026, 1, 600m),    // -500
                Income(2026, 2, 700m),                            // +700
            ];

            // Clamping January to 0 would report 700 here and silently erase the shortfall for good.
            OpeningBalanceCalculator.Calculate(totals, 2026, 3).Should().Be(200m);
        }

        [Fact]
        public void Calculate_ShouldCrossYearBoundaries()
        {
            MonthlyTotal[] totals = [Income(2025, 12, 900m), Expense(2026, 1, 100m)];

            OpeningBalanceCalculator.Calculate(totals, 2026, 1).Should().Be(900m);
            OpeningBalanceCalculator.Calculate(totals, 2026, 2).Should().Be(800m);
        }

        [Fact]
        public void Calculate_ShouldNotDependOnInputOrder()
        {
            MonthlyTotal[] totals = [Expense(2026, 2, 900m), Income(2026, 1, 1000m), Income(2026, 2, 1000m)];

            OpeningBalanceCalculator.Calculate(totals, 2026, 3).Should().Be(1100m);
        }
    }
}
