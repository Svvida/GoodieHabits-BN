using Application.Interfaces.Quests;
using Application.Services.Quests;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Factories;

namespace Tests.Services
{
    public class QuestResetServiceTests
    {
        private readonly IQuestResetService _questResetService;
        private readonly Mock<ILogger<QuestResetService>> _loggerMock;

        public QuestResetServiceTests()
        {
            _loggerMock = new Mock<ILogger<QuestResetService>>();
            _questResetService = new QuestResetService(_loggerMock.Object);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNextMonday_WhenTodayIsMonday()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            //Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(DayOfWeek.Monday, nextResetTime.Value.DayOfWeek);
            Assert.True(nextResetTime.Value > DateTime.UtcNow.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNextAvailableResetDay_WhenTodayIsNotTheResetDay()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);
            var questWithDifferentLastCompleted = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Wednesday);

            // Act
            DateTime? nextResetTimeForMondayQuest = _questResetService.GetNextResetTimeUtc(quest);
            DateTime? nextResetTimeForWednesdayQuest = _questResetService.GetNextResetTimeUtc(questWithDifferentLastCompleted);

            // Assert for Monday quest (should reset next Monday)
            Assert.NotNull(nextResetTimeForMondayQuest);
            Assert.Equal(DayOfWeek.Monday, nextResetTimeForMondayQuest.Value.DayOfWeek);

            // Assert for Wednesday quest (should reset next Wednesday)
            Assert.NotNull(nextResetTimeForWednesdayQuest);
            Assert.Equal(DayOfWeek.Wednesday, nextResetTimeForWednesdayQuest.Value.DayOfWeek);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldAdjustForTimeZoneOffset_WhenCalculatingResetDay()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday, timeZone: "Europe/Warsaw");

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.NotNull(nextResetTime);
            // Expect the next reset time to be Sunday because Europe/Warsaw is UTC+1/UTC+2
            Assert.Equal(DayOfWeek.Sunday, nextResetTime.Value.DayOfWeek);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenLastCompletedAtIsNull()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);
            quest.LastCompletedAt = null;

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.Null(nextResetTime);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenNextResetIsAfterEndDate()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);
            quest.EndDate = DateTime.UtcNow.AddDays(-1);
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.Null(nextResetTime);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNextDay_WhenIsDailyQuest()
        {
            // Arrange
            var quest = QuestFactory.CreateDailyQuest(accountId: 1, timeZone: "Etc/UTC");
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(DateTime.UtcNow.Date.AddDays(1), nextResetTime.Value.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnSameDayForDailyQuest_WhenTimezoneOffsetIsApplied()
        {
            // Arrange
            var quest = QuestFactory.CreateDailyQuest(accountId: 1, timeZone: "Europe/Warsaw");
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(DateTime.UtcNow.Date, nextResetTime.Value.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenDailyQuestEndDateIsBeforeNextReset()
        {
            // Arrange
            var quest = QuestFactory.CreateDailyQuest(accountId: 1, timeZone: "Etc/UTC");
            quest.EndDate = DateTime.UtcNow.Date;
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.Null(nextResetTime);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenDailyQuestEndDateIsBeforeNextResetAndTimezoneOffsetIsApplied()
        {
            // Arrange
            var quest = QuestFactory.CreateDailyQuest(accountId: 1, timeZone: "Europe/Warsaw");
            quest.EndDate = DateTime.UtcNow.Date;
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.Null(nextResetTime);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldResetOnNextMonthStartDay_WhenQuestIsMonthly()
        {
            // Arrange
            var quest = QuestFactory.CreateMonthlyQuest(accountId: 1, startDay: 10, endDay: 20, timeZone: "Etc/UTC");
            quest.LastCompletedAt = new DateTime(2025, 3, 15);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(new DateTime(2025, 4, 10), nextResetTime.Value.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldAdjustForTimeZone_WhenMonthlyResetDayIsCalculated()
        {
            // Arrange
            var quest = QuestFactory.CreateMonthlyQuest(accountId: 1, startDay: 10, endDay: 20, timeZone: "Europe/Warsaw");
            quest.LastCompletedAt = new DateTime(2025, 3, 15);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.NotNull(nextResetTime);
            // Expect the next reset time to be 9th of April because Europe/Warsaw is UTC+1/UTC+2
            Assert.Equal(new DateTime(2025, 4, 9), nextResetTime.Value.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenQuestEndDateHasPassed()
        {
            // Arrange
            var quest = QuestFactory.CreateMonthlyQuest(accountId: 1, startDay: 10, endDay: 20, timeZone: "Etc/UTC");
            quest.LastCompletedAt = new DateTime(2025, 3, 15);
            quest.EndDate = new DateTime(2025, 3, 10);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.Null(nextResetTime);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldResetOnLastDayOfMonth_WhenStartDayExceedsMonthDays()
        {
            // Arrange
            var quest = QuestFactory.CreateMonthlyQuest(accountId: 1, startDay: 31, endDay: 31, timeZone: "Etc/UTC");
            quest.LastCompletedAt = new DateTime(2025, 1, 31);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(new DateTime(2025, 2, 28), nextResetTime.Value.Date);

        }
    }
}
