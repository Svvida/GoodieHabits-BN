using Domain.Calculators;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Calculators
{
    /// <summary>
    /// Phase 12.3. Pure month arithmetic — no DB, which matters because the InMemory provider would pass
    /// date-boundary logic that real SQL gets wrong.
    /// </summary>
    public class RecurrenceCalculatorTests
    {
        private static RecurringTransaction Template(int dayOfMonth, DateOnly createdOn) =>
            RecurringTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 40m, dayOfMonth, createdOn);

        [Fact]
        public void GetMissingOccurrences_ShouldReturnNothing_InTheCreationMonth()
        {
            var template = Template(10, new DateOnly(2026, 3, 20));

            // The client has already created this month's transaction alongside the template.
            RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 3, 31)).Should().BeEmpty();
        }

        [Fact]
        public void GetMissingOccurrences_ShouldStartTheMonthAfterCreation()
        {
            var template = Template(10, new DateOnly(2026, 3, 20));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 4, 5));

            result.Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 4, 10));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldBackfillEveryMonthAfterALongGap()
        {
            var template = Template(15, new DateOnly(2025, 11, 1));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 3, 7));

            result.Should().Equal(
                new DateOnly(2025, 12, 15),
                new DateOnly(2026, 1, 15),
                new DateOnly(2026, 2, 15),
                new DateOnly(2026, 3, 15));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldNeverReachIntoTheFuture()
        {
            var template = Template(28, new DateOnly(2026, 1, 5));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 2, 1));

            // February's row is generated on the 1st even though the 28th hasn't arrived; March is not.
            result.Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 2, 28));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldClampToAShortMonth()
        {
            var template = Template(31, new DateOnly(2026, 1, 5));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 4, 30));

            result.Should().Equal(
                new DateOnly(2026, 2, 28),   // 2026 is not a leap year
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 4, 30));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldClampToLeapFebruary()
        {
            var template = Template(30, new DateOnly(2028, 1, 5));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2028, 2, 15));

            result.Should().ContainSingle().Which.Should().Be(new DateOnly(2028, 2, 29));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldReturnNothing_WhenInactive()
        {
            var template = Template(10, new DateOnly(2026, 1, 5));
            template.SetActive(false, new DateOnly(2026, 1, 5));

            RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 6, 1)).Should().BeEmpty();
        }

        [Fact]
        public void GetMissingOccurrences_ShouldNotBackfillDormantMonths_AfterResuming()
        {
            var template = Template(10, new DateOnly(2026, 1, 5));
            template.SetActive(false, new DateOnly(2026, 1, 20));

            // Paused January-April, resumed in May: a pause means "don't charge me", not "charge me later".
            template.SetActive(true, new DateOnly(2026, 5, 3));

            RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 5, 20)).Should().BeEmpty();
            RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 6, 20))
                .Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 6, 10));
        }

        [Fact]
        public void GetMissingOccurrences_ShouldCrossAYearBoundary()
        {
            var template = Template(3, new DateOnly(2025, 11, 20));

            var result = RecurrenceCalculator.GetMissingOccurrences(template, new DateOnly(2026, 1, 4));

            result.Should().Equal(new DateOnly(2025, 12, 3), new DateOnly(2026, 1, 3));
        }

        [Theory]
        [InlineData(2026, 2, 31, 28)]
        [InlineData(2028, 2, 31, 29)]
        [InlineData(2026, 4, 31, 30)]
        [InlineData(2026, 5, 31, 31)]
        [InlineData(2026, 5, 1, 1)]
        public void ClampToMonth_ShouldRespectMonthLength(int year, int month, int dayOfMonth, int expectedDay)
        {
            RecurrenceCalculator.ClampToMonth(year, month, dayOfMonth)
                .Should().Be(new DateOnly(year, month, expectedDay));
        }
    }
}
