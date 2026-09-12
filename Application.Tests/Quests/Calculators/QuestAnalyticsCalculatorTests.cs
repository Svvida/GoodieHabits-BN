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
        public void ByWeekday_ShouldCountTheDaysTheHabitActuallyHappensOn()
        {
            var quest = QuestTestFactory.Daily();
            var nowUtc = new DateTime(2020, 8, 20, 9, 0, 0, DateTimeKind.Utc);
            var today = new DateOnly(2020, 8, 20);

            // Two Tuesdays done, no Mondays.
            quest.AddCompletion(nowUtc, today, completedOn: new DateOnly(2020, 8, 18));
            quest.AddCompletion(nowUtc, today, completedOn: new DateOnly(2020, 8, 19));

            var breakdown = QuestAnalyticsCalculator.ByWeekday(
                quest.Completions, quest.QuestOccurrences, new DateOnly(2020, 8, 17), today);

            var tuesday = breakdown.Single(b => b.Weekday == WeekdayEnum.Tuesday);
            tuesday.Completions.Should().Be(1);
            // The denominator the FE needs: one Tuesday in a 2020-08-17..2020-08-20 window.
            tuesday.DaysInRange.Should().Be(1);
        }

        [Fact]
        public void ByWeekday_ShouldWorkForAWeeklyTargetHabit()
        {
            // The case the old implementation could not report on at all: the period is the week, but the
            // question "which days do I actually train?" is about days.
            var quest = QuestTestFactory.TimesPerWeek(2);
            var nowUtc = new DateTime(2020, 8, 7, 9, 0, 0, DateTimeKind.Utc);
            var today = new DateOnly(2020, 8, 7);

            quest.InitializePeriods(today);
            quest.AddCompletion(nowUtc, today, completedOn: new DateOnly(2020, 8, 5));
            quest.AddCompletion(nowUtc, today, completedOn: today);

            var breakdown = QuestAnalyticsCalculator.ByWeekday(
                quest.Completions, quest.QuestOccurrences, new DateOnly(2020, 8, 3), today);

            breakdown.Should().HaveCount(2);
            breakdown.Sum(b => b.Completions).Should().Be(2);
            // A Week schedule pins no weekdays, so "how many Tuesdays was I due?" has no answer.
            breakdown.Should().OnlyContain(b => b.DaysScheduled == null);
        }

        [Fact]
        public void ByHourOfDay_ShouldGroupByTheSnapshottedLocalTime()
        {
            var quest = QuestTestFactory.Daily(target: 2);
            var nowUtc = new DateTime(2020, 8, 5, 9, 0, 0, DateTimeKind.Utc);

            quest.AddCompletion(nowUtc, Today, localTime: new TimeOnly(7, 30));
            quest.AddCompletion(nowUtc, Today, localTime: new TimeOnly(21, 15));

            var byHour = QuestAnalyticsCalculator.ByHourOfDay(quest.Completions);

            byHour.Select(h => h.Hour).Should().Equal(7, 21);
        }

        [Fact]
        public void ByHourOfDay_ShouldSkipCompletionsWithNoRecordedLocalTime()
        {
            var quest = QuestTestFactory.Daily();
            quest.AddCompletion(new DateTime(2020, 8, 5, 9, 0, 0, DateTimeKind.Utc), Today);

            QuestAnalyticsCalculator.ByHourOfDay(quest.Completions)
                .Should().BeEmpty("migrated rows have no local time and must not be guessed at");
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
