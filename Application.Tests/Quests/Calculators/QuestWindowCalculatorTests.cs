using Domain.Calculators;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    public class QuestWindowCalculatorTests
    {
        [Fact]
        public void Daily_ShouldEmitOneSingleDayWindowPerDay()
        {
            var quest = QuestTestFactory.Daily();

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 3));

            windows.Should().HaveCount(3);
            windows.Should().OnlyContain(w => w.Start == w.End);
            windows.Select(w => w.Start).Should().Equal(
                new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 2), new DateOnly(2020, 8, 3));
        }

        [Fact]
        public void Daily_ShouldEmitNothing_WhenRangeIsInverted()
        {
            var quest = QuestTestFactory.Daily();

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2020, 8, 3), new DateOnly(2020, 8, 1));

            windows.Should().BeEmpty();
        }

        [Fact]
        public void Weekly_ShouldEmitWindowsOnlyOnScheduledWeekdays()
        {
            // 2020-08-01 is a Saturday.
            var quest = QuestTestFactory.Weekly([WeekdayEnum.Monday, WeekdayEnum.Wednesday]);

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 14));

            windows.Select(w => w.Start).Should().Equal(
                new DateOnly(2020, 8, 3),  // Mon
                new DateOnly(2020, 8, 5),  // Wed
                new DateOnly(2020, 8, 10), // Mon
                new DateOnly(2020, 8, 12));// Wed
        }

        [Fact]
        public void Monthly_ShouldEmitOneRangeWindowPerMonth()
        {
            var quest = QuestTestFactory.Monthly(startDay: 5, endDay: 10);

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 9, 30));

            windows.Should().HaveCount(2);
            windows[0].Start.Should().Be(new DateOnly(2020, 8, 5));
            windows[0].End.Should().Be(new DateOnly(2020, 8, 10));
            windows[1].Start.Should().Be(new DateOnly(2020, 9, 5));
            windows[1].End.Should().Be(new DateOnly(2020, 9, 10));
        }

        [Fact]
        public void Monthly_ShouldClampToShortMonths()
        {
            var quest = QuestTestFactory.Monthly(startDay: 30, endDay: 31);

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2021, 2, 1), new DateOnly(2021, 2, 28));

            windows.Should().ContainSingle();
            windows[0].Start.Should().Be(new DateOnly(2021, 2, 28));
            windows[0].End.Should().Be(new DateOnly(2021, 2, 28));
        }

        [Fact]
        public void Monthly_ShouldHandleLeapFebruary()
        {
            var quest = QuestTestFactory.Monthly(startDay: 28, endDay: 31);

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 29));

            windows.Should().ContainSingle();
            windows[0].End.Should().Be(new DateOnly(2024, 2, 29));
        }

        [Fact]
        public void NonRepeatableQuest_ShouldEmitNothing()
        {
            var quest = Domain.Models.Quest.Create(
                "One time", QuestTestFactory.Profile(), QuestTypeEnum.OneTime, QuestTestFactory.DefaultNowUtc);

            var windows = QuestWindowCalculator.GenerateWindows(quest, new DateOnly(2020, 8, 1), new DateOnly(2020, 8, 5));

            windows.Should().BeEmpty();
        }
    }
}
