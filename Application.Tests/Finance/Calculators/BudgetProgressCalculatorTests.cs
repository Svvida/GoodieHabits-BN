using Domain.Calculators;
using FluentAssertions;

namespace Application.Tests.Finance.Calculators
{
    public class BudgetProgressCalculatorTests
    {
        [Fact]
        public void Calculate_UnderBudget()
        {
            var progress = BudgetProgressCalculator.Calculate(limit: 100m, spent: 40m);

            progress.Remaining.Should().Be(60m);
            progress.PercentUsed.Should().Be(40m);
            progress.IsOverBudget.Should().BeFalse();
        }

        [Fact]
        public void Calculate_OverBudget()
        {
            var progress = BudgetProgressCalculator.Calculate(limit: 100m, spent: 150m);

            progress.Remaining.Should().Be(-50m);
            progress.PercentUsed.Should().Be(150m);
            progress.IsOverBudget.Should().BeTrue();
        }

        [Fact]
        public void Calculate_ExactlyAtLimit_IsNotOver()
        {
            var progress = BudgetProgressCalculator.Calculate(limit: 100m, spent: 100m);

            progress.Remaining.Should().Be(0m);
            progress.PercentUsed.Should().Be(100m);
            progress.IsOverBudget.Should().BeFalse();
        }

        [Fact]
        public void Calculate_ZeroLimit_DoesNotDivideByZero()
        {
            var progress = BudgetProgressCalculator.Calculate(limit: 0m, spent: 50m);

            progress.PercentUsed.Should().Be(0m);
            progress.Remaining.Should().Be(-50m);
            progress.IsOverBudget.Should().BeTrue();
        }
    }
}
