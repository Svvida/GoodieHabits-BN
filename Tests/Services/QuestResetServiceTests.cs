using Application.Services.Quests;
using Domain.Calculators;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Factories;

namespace Tests.Services
{
    public class QuestResetServiceTests
    {
        private readonly QuestResetService _questResetService;
        private readonly Mock<ILogger<QuestResetService>> _loggerMock;
        private readonly Mock<NodaTime.IClock> _clockMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public QuestResetServiceTests()
        {
            _loggerMock = new Mock<ILogger<QuestResetService>>();
            _clockMock = new Mock<NodaTime.IClock>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _questResetService = new QuestResetService(_loggerMock.Object, _clockMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnNextMonday_WhenTodayIsMonday()
        {
            // Arrange
            SetCurrentUtcTime(new DateTime(2025, 5, 12));
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            //Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(new DateTime(2025, 5, 19), nextResetTime.Value.Date);
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
        public void GetNextResetTimeUtc_ShouldReturnNull_WhenNextResetIsAfterEndDate()
        {
            // Arrange
            var quest = QuestFactory.CreateWeeklyQuest(accountId: 1, resetDay: WeekdayEnum.Monday);
            SetCurrentUtcTime(new DateTime(2025, 5, 12));
            quest.EndDate = new DateTime(2025, 5, 14);
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
            SetCurrentUtcTime(new DateTime(2025, 3, 15));
            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);
            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(new DateTime(2025, 3, 16), nextResetTime.Value.Date);
        }

        [Fact]
        public void GetNextResetTimeUtc_ShouldReturnSameDayForDailyQuest_WhenTimezoneOffsetIsApplied()
        {
            // Arrange
            var quest = QuestFactory.CreateDailyQuest(accountId: 1, timeZone: "Europe/Warsaw");
            SetCurrentUtcTime(DateTime.UtcNow.Date);
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
            SetCurrentUtcTime(new DateTime(2025, 3, 15));
            quest.EndDate = new DateTime(2025, 3, 15);
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
            SetCurrentUtcTime(new DateTime(2025, 3, 15));
            quest.EndDate = new DateTime(2025, 3, 15);
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
            SetCurrentUtcTime(new DateTime(2025, 3, 15));

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
            SetCurrentUtcTime(new DateTime(2025, 3, 10));

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
            SetCurrentUtcTime(new DateTime(2025, 3, 15));
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
            SetCurrentUtcTime(new DateTime(2025, 1, 31));

            // Act
            DateTime? nextResetTime = _questResetService.GetNextResetTimeUtc(quest);

            // Assert
            Assert.NotNull(nextResetTime);
            Assert.Equal(new DateTime(2025, 2, 28), nextResetTime.Value.Date);

        }

        private void SetCurrentUtcTime(DateTime utcNow)
        {
            var instant = NodaTime.Instant.FromDateTimeUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(instant);
        }
    }
}
