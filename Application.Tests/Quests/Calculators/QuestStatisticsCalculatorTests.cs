using Domain.Calculators;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    public class QuestStatisticsCalculatorTests
    {
        private static readonly DateOnly Today = new(2020, 8, 5);
        private static readonly DateTime CompletedAt = new(2020, 8, 1, 9, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void ShouldCountCompletionsAndFailures()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 2));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3), completedAtUtc: CompletedAt);

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.OccurrenceCount.Should().Be(3);
            stats.CompletionCount.Should().Be(2);
            stats.FailureCount.Should().Be(1);
        }

        [Fact]
        public void InProgressPeriod_ShouldNotCountAsFailure()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, Today);

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.OccurrenceCount.Should().Be(1);
            stats.FailureCount.Should().Be(0);
            stats.CompletionCount.Should().Be(0);
        }

        [Fact]
        public void FuturePeriod_ShouldNotCountAsFailure()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, Today.AddDays(3));

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.FailureCount.Should().Be(0);
        }

        [Fact]
        public void CurrentStreak_ShouldCountBackwardsUntilAMissedPeriod()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 2));
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 4), completedAtUtc: CompletedAt);

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.CurrentStreak.Should().Be(2);
            stats.LongestStreak.Should().Be(2);
        }

        [Fact]
        public void CurrentStreak_ShouldSurviveAnUncompletedInProgressPeriod()
        {
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 4), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, Today); // today, not done yet

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.CurrentStreak.Should().Be(2, "an unfinished day should not break the streak");
        }

        [Fact]
        public void LongestStreak_ShouldKeepTheBestRunEvenAfterItBreaks()
        {
            var quest = QuestTestFactory.Daily();
            foreach (var day in new[] { 1, 2, 3 })
                QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, day), completedAtUtc: CompletedAt);

            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 4));

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.CurrentStreak.Should().Be(0);
            stats.LongestStreak.Should().Be(3);
        }

        [Fact]
        public void ShouldReportTheMostRecentCompletionInstant()
        {
            var later = new DateTime(2020, 8, 3, 20, 0, 0, DateTimeKind.Utc);
            var quest = QuestTestFactory.Daily();
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), completedAtUtc: CompletedAt);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 3), completedAtUtc: later);

            var stats = QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, Today);

            stats.LastCompletedAt.Should().Be(later);
        }

        [Fact]
        public void MonthlyPeriod_ShouldOnlyFailOnceItsWholeRangeHasElapsed()
        {
            var quest = QuestTestFactory.Monthly(startDay: 1, endDay: 10);
            QuestTestFactory.AddPeriod(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 10));

            QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, new DateOnly(2020, 8, 5))
                .FailureCount.Should().Be(0, "the window is still open");

            QuestStatisticsCalculator.Calculate(quest.QuestOccurrences, new DateOnly(2020, 8, 11))
                .FailureCount.Should().Be(1);
        }
    }
}
