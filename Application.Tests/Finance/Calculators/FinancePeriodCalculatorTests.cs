using Domain.Calculators;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Application.Tests.Finance.Calculators
{
    public class FinancePeriodCalculatorTests
    {
        [Fact]
        public void ForMonth_ShouldReturnFirstAndLastDay()
        {
            var (start, end) = FinancePeriodCalculator.ForMonth(2026, 1);
            start.Should().Be(new DateOnly(2026, 1, 1));
            end.Should().Be(new DateOnly(2026, 1, 31));
        }

        [Fact]
        public void ForMonth_ShouldHandleLeapFebruary()
        {
            var (_, end) = FinancePeriodCalculator.ForMonth(2024, 2);
            end.Should().Be(new DateOnly(2024, 2, 29));
        }

        [Fact]
        public void ForMonth_ShouldHandleDecember()
        {
            var (start, end) = FinancePeriodCalculator.ForMonth(2026, 12);
            start.Should().Be(new DateOnly(2026, 12, 1));
            end.Should().Be(new DateOnly(2026, 12, 31));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void ForMonth_ShouldThrow_WhenMonthOutOfRange(int month)
        {
            var act = () => FinancePeriodCalculator.ForMonth(2026, month);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void ForYear_ShouldSpanWholeYear()
        {
            var (start, end) = FinancePeriodCalculator.ForYear(2026);
            start.Should().Be(new DateOnly(2026, 1, 1));
            end.Should().Be(new DateOnly(2026, 12, 31));
        }

        [Fact]
        public void ForPeriod_Monthly_ShouldMatchForMonth()
        {
            var period = FinancePeriodCalculator.ForPeriod(BudgetPeriodEnum.Monthly, 2026, 5);
            period.Should().Be(FinancePeriodCalculator.ForMonth(2026, 5));
        }

        [Fact]
        public void ForPeriod_Yearly_ShouldMatchForYear()
        {
            var period = FinancePeriodCalculator.ForPeriod(BudgetPeriodEnum.Yearly, 2026, null);
            period.Should().Be(FinancePeriodCalculator.ForYear(2026));
        }
    }
}
