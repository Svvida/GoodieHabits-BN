using Domain.Calculators;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    public class NextResetDateCalculatorTests
    {
        [Fact]
        public void Daily_ShouldResetAtTheUsersNextLocalMidnight()
        {
            var quest = QuestTestFactory.Daily(QuestTestFactory.Profile("Europe/Warsaw"));

            // 2020-08-02 12:00Z is 14:00 local (UTC+2); next local midnight is 2020-08-02 22:00Z.
            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 8, 2, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2020, 8, 2, 22, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Daily_ShouldAccountForDstWhenTheOffsetChangesOvernight()
        {
            var quest = QuestTestFactory.Daily(QuestTestFactory.Profile("Europe/Warsaw"));

            // Warsaw falls back 2020-10-25 (UTC+2 -> UTC+1), so that day's midnight boundary shifts.
            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 10, 25, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2020, 10, 25, 23, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Weekly_ShouldPickTheNextScheduledWeekday()
        {
            // 2020-08-02 is a Sunday.
            var quest = QuestTestFactory.Weekly(
                [WeekdayEnum.Monday, WeekdayEnum.Friday],
                QuestTestFactory.Profile("Etc/UTC"));

            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 8, 2, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2020, 8, 3, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Weekly_ShouldWrapToNextWeek_WhenTodayIsTheOnlyScheduledDay()
        {
            // 2020-08-03 is a Monday.
            var quest = QuestTestFactory.Weekly([WeekdayEnum.Monday], QuestTestFactory.Profile("Etc/UTC"));

            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 8, 3, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Monthly_ShouldResetOnTheStartDayOfTheFollowingMonth()
        {
            var quest = QuestTestFactory.Monthly(startDay: 5, endDay: 10, profile: QuestTestFactory.Profile("Etc/UTC"));

            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 8, 6, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2020, 9, 5, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Monthly_ShouldClampTheStartDayToAShortMonth()
        {
            var quest = QuestTestFactory.Monthly(startDay: 31, endDay: 31, profile: QuestTestFactory.Profile("Etc/UTC"));

            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2021, 1, 31, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().Be(new DateTime(2021, 2, 28, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ShouldReturnNull_WhenTheResetWouldFallAfterTheQuestsEndDate()
        {
            var quest = QuestTestFactory.Daily(
                QuestTestFactory.Profile("Etc/UTC"),
                endDate: new DateOnly(2020, 8, 2));

            var reset = NextResetDateCalculator.Calculate(quest, new DateTime(2020, 8, 2, 12, 0, 0, DateTimeKind.Utc));

            reset.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnNull_ForNonRepeatableQuests()
        {
            var quest = Domain.Models.Quest.Create(
                "One time", QuestTestFactory.Profile("Etc/UTC"), QuestTypeEnum.OneTime, QuestTestFactory.DefaultNowUtc);

            NextResetDateCalculator.Calculate(quest, QuestTestFactory.DefaultNowUtc).Should().BeNull();
        }
    }
}
