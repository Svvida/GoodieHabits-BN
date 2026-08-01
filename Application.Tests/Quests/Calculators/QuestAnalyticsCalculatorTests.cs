using Domain.Calculators;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    public class QuestAnalyticsCalculatorTests
    {
        private static readonly DateOnly Today = new(2020, 8, 5);
        private static readonly DateTime CompletedAt = new(2020, 8, 1, 9, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void CompletionRate_ShouldIgnorePendingPeriods()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 2));
            QuestTestFactory.AddPeriod(quest, Today); // in progress

            var summary = QuestAnalyticsCalculator.Summarize(quest.QuestOccurrences, Today);

            summary.TotalPeriods.Should().Be(3);
            summary.EvaluatedPeriods.Should().Be(2);
            summary.PendingPeriods.Should().Be(1);
            summary.CompletionRate.Should().Be(0.5, "today is not a failure yet");
        }

        [Fact]
        public void CompletionRate_ShouldBeNull_WhenNothingHasBeenEvaluated()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, Today.AddDays(1));

            var summary = QuestAnalyticsCalculator.Summarize(quest.QuestOccurrences, Today);

            summary.CompletionRate.Should().BeNull();
        }

        [Fact]
        public void Summarize_ShouldReturnEmpty_WhenThereAreNoOccurrences()
        {
            QuestAnalyticsCalculator.Summarize([], Today)
                .Should().BeEquivalentTo(Domain.ValueObjects.QuestAnalyticsSummary.Empty);
        }

        [Theory]
        [InlineData(QuestPeriodOutcomeEnum.Completed, true, 0)]
        [InlineData(QuestPeriodOutcomeEnum.Missed, false, -2)]
        [InlineData(QuestPeriodOutcomeEnum.Pending, false, 0)]
        public void OutcomeOf_ShouldClassifyPeriods(QuestPeriodOutcomeEnum expected, bool completed, int dayOffset)
        {
            var quest = QuestTestFactory.Daily();
            var occurrence = QuestTestFactory.AddPeriod(
                quest,
                Today.AddDays(dayOffset),
                completedAtUtc: completed ? CompletedAt : null);

            QuestAnalyticsCalculator.OutcomeOf(occurrence, Today).Should().Be(expected);
        }

        [Fact]
        public void Bucket_ShouldGroupByIsoWeekStartingMonday()
        {
            var quest = QuestTestFactory.Daily();
            // 2020-08-03 is a Monday; 2020-08-09 the Sunday closing that week.
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 4));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 10), completedAtUtc: CompletedAt);

            var buckets = QuestAnalyticsCalculator.Bucket(
                quest.QuestOccurrences, AnalyticsGranularityEnum.Week, new DateOnly(2020, 8, 20));

            buckets.Should().HaveCount(2);
            buckets[0].BucketStart.Should().Be(new DateOnly(2020, 8, 3));
            buckets[0].BucketEnd.Should().Be(new DateOnly(2020, 8, 9));
            buckets[0].CompletionRate.Should().Be(0.5);
            buckets[1].BucketStart.Should().Be(new DateOnly(2020, 8, 10));
            buckets[1].CompletionRate.Should().Be(1.0);
        }

        [Fact]
        public void Bucket_ShouldGroupByCalendarMonth()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 31), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 9, 1));

            var buckets = QuestAnalyticsCalculator.Bucket(
                quest.QuestOccurrences, AnalyticsGranularityEnum.Month, new DateOnly(2020, 10, 1));

            buckets.Should().HaveCount(2);
            buckets[0].BucketStart.Should().Be(new DateOnly(2020, 8, 1));
            buckets[0].BucketEnd.Should().Be(new DateOnly(2020, 8, 31));
            buckets[1].BucketStart.Should().Be(new DateOnly(2020, 9, 1));
            buckets[1].BucketEnd.Should().Be(new DateOnly(2020, 9, 30));
        }

        [Fact]
        public void ByWeekday_ShouldSurfaceTheDayTheHabitKeepsSlipping()
        {
            var quest = QuestTestFactory.Daily();
            // Mondays missed, Tuesdays done.
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 10));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 4), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 11), completedAtUtc: CompletedAt);

            var breakdown = QuestAnalyticsCalculator.ByWeekday(quest.QuestOccurrences, new DateOnly(2020, 8, 20));

            breakdown.Single(b => b.Weekday == WeekdayEnum.Monday).CompletionRate.Should().Be(0.0);
            breakdown.Single(b => b.Weekday == WeekdayEnum.Tuesday).CompletionRate.Should().Be(1.0);
        }

        [Fact]
        public void ByWeekday_ShouldSkipMultiDayPeriods()
        {
            var quest = QuestTestFactory.Monthly(startDay: 1, endDay: 10);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 10));

            QuestAnalyticsCalculator.ByWeekday(quest.QuestOccurrences, Today)
                .Should().BeEmpty("a weekday breakdown is meaningless for a period spanning a range");
        }

        [Fact]
        public void ToCalendar_ShouldReturnOneOrderedEntryPerPeriod()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 2));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), completedAtUtc: CompletedAt);

            var calendar = QuestAnalyticsCalculator.ToCalendar(quest.QuestOccurrences, Today);

            calendar.Select(c => c.PeriodStart).Should().Equal(new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 2));
            calendar[0].Outcome.Should().Be(QuestPeriodOutcomeEnum.Completed);
            calendar[0].CompletedAtUtc.Should().Be(CompletedAt);
            calendar[1].Outcome.Should().Be(QuestPeriodOutcomeEnum.Missed);
        }
    }
}
