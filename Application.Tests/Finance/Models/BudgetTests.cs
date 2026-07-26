using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Models
{
    public class BudgetTests
    {
        [Fact]
        public void Create_Monthly_ShouldSetFields()
        {
            var budget = Budget.Create(1, 5, BudgetPeriodEnum.Monthly, 2026, 3, 500m);

            budget.UserProfileId.Should().Be(1);
            budget.CategoryId.Should().Be(5);
            budget.Period.Should().Be(BudgetPeriodEnum.Monthly);
            budget.Year.Should().Be(2026);
            budget.Month.Should().Be(3);
            budget.LimitAmount.Should().Be(500m);
        }

        [Fact]
        public void Create_Monthly_ShouldThrow_WhenMonthMissing()
        {
            var act = () => Budget.Create(1, null, BudgetPeriodEnum.Monthly, 2026, null, 500m);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void Create_Monthly_ShouldThrow_WhenMonthOutOfRange(int month)
        {
            var act = () => Budget.Create(1, null, BudgetPeriodEnum.Monthly, 2026, month, 500m);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_Yearly_ShouldThrow_WhenMonthProvided()
        {
            var act = () => Budget.Create(1, null, BudgetPeriodEnum.Yearly, 2026, 6, 500m);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_Yearly_ShouldHaveNullMonth()
        {
            var budget = Budget.Create(1, null, BudgetPeriodEnum.Yearly, 2026, null, 5000m);
            budget.Month.Should().BeNull();
            budget.CategoryId.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Create_ShouldThrow_WhenLimitNotPositive(decimal limit)
        {
            var act = () => Budget.Create(1, null, BudgetPeriodEnum.Monthly, 2026, 1, limit);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void UpdateLimit_ShouldThrow_WhenNotPositive()
        {
            var budget = Budget.Create(1, null, BudgetPeriodEnum.Monthly, 2026, 1, 100m);
            var act = () => budget.UpdateLimit(0);
            act.Should().Throw<InvalidArgumentException>();
        }
    }
}
