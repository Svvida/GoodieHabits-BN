using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Quests
{
    /// <summary>
    /// Regression cover for the bug that motivated moving occurrences to calendar periods: because the old
    /// model stored UTC instants derived from whatever timezone the profile held at generation time, a user
    /// changing timezone produced a second occurrence for a local day they already had (travelling west) or
    /// skipped a local day entirely (travelling east).
    /// <para>
    /// Generation now takes the user's local <c>today</c>, resolved through
    /// <see cref="Domain.Models.UserProfile.LocalDateOn"/>, and the arithmetic below it is pure calendar
    /// maths — which is what makes these cases impossible rather than merely fixed.
    /// </para>
    /// </summary>
    public class QuestOccurrenceGenerationTests
    {
        private static readonly DateTime Aug2Noon = new(2020, 8, 2, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime Aug3Noon = new(2020, 8, 3, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void TravellingWest_ShouldNotCreateASecondPeriodForTheSameLocalDay()
        {
            var profile = QuestTestFactory.Profile("Europe/Warsaw");
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 8, 1));

            quest.InitializePeriods(profile.LocalDateOn(Aug2Noon));
            var periodsBefore = quest.QuestOccurrences.Select(o => o.PeriodStart).ToList();

            // The user lands in New York; RefreshAccessToken updates the profile timezone mid-flight.
            profile.UpdateTimeZone("America/New_York");
            quest.GenerateMissingPeriodsOn(profile.LocalDateOn(Aug2Noon));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().OnlyHaveUniqueItems();
            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().BeEquivalentTo(periodsBefore);
        }

        [Fact]
        public void TravellingEast_ShouldNotSkipALocalDay()
        {
            var profile = QuestTestFactory.Profile("America/New_York");
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 8, 1));

            quest.InitializePeriods(profile.LocalDateOn(Aug2Noon));

            profile.UpdateTimeZone("Europe/Warsaw");
            quest.GenerateMissingPeriodsOn(profile.LocalDateOn(Aug3Noon));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().Equal(
                new DateOnly(2020, 8, 1),
                new DateOnly(2020, 8, 2),
                new DateOnly(2020, 8, 3));
        }

        [Fact]
        public void GenerateMissingPeriods_ShouldBeIdempotent()
        {
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 8, 1));
            var today = profile.LocalDateOn(Aug3Noon);

            quest.InitializePeriods(today);
            int firstPass = quest.QuestOccurrences.Count;

            quest.GenerateMissingPeriodsOn(today).Should().Be(0);
            quest.QuestOccurrences.Should().HaveCount(firstPass);
        }

        [Fact]
        public void Generation_ShouldNotProducePeriodsBeforeStartDate()
        {
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 8, 2));

            quest.InitializePeriods(profile.LocalDateOn(Aug3Noon));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().Equal(
                new DateOnly(2020, 8, 2),
                new DateOnly(2020, 8, 3));
        }

        [Fact]
        public void Generation_ShouldNotProducePeriodsAfterEndDate()
        {
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.Daily(
                profile,
                startDate: new DateOnly(2020, 8, 1),
                endDate: new DateOnly(2020, 8, 2));

            quest.InitializePeriods(profile.LocalDateOn(Aug3Noon));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().Equal(
                new DateOnly(2020, 8, 1),
                new DateOnly(2020, 8, 2));
        }

        [Fact]
        public void DstTransition_ShouldStillProduceExactlyOnePeriodPerLocalDay()
        {
            // Europe/Warsaw falls back on 2020-10-25, making that local day 25 hours long.
            var profile = QuestTestFactory.Profile("Europe/Warsaw");
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 10, 24));

            quest.InitializePeriods(profile.LocalDateOn(new DateTime(2020, 10, 26, 12, 0, 0, DateTimeKind.Utc)));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().Equal(
                new DateOnly(2020, 10, 24),
                new DateOnly(2020, 10, 25),
                new DateOnly(2020, 10, 26));
        }

        [Fact]
        public void Completing_ShouldCreditThePeriodCoveringTheUsersLocalToday()
        {
            var profile = QuestTestFactory.Profile("Pacific/Auckland");
            var quest = QuestTestFactory.Daily(profile, startDate: new DateOnly(2020, 8, 1));

            // 22:00Z on 2 Aug is already 3 Aug in Auckland (UTC+12).
            var nowUtc = new DateTime(2020, 8, 2, 22, 0, 0, DateTimeKind.Utc);
            var today = profile.LocalDateOn(nowUtc);

            quest.InitializePeriods(today);
            quest.AddCompletion(nowUtc, today);

            var completed = quest.QuestOccurrences.Single(o => o.IsCompleted);
            completed.PeriodStart.Should().Be(new DateOnly(2020, 8, 3));
            completed.IsBackfilled.Should().BeFalse();
        }

        [Fact]
        public void WeekdayScopedQuest_ShouldOnlyGeneratePeriodsOnScheduledDays()
        {
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.OnWeekdays(
                [WeekdayEnum.Monday],
                profile,
                startDate: new DateOnly(2020, 8, 1));

            quest.InitializePeriods(profile.LocalDateOn(new DateTime(2020, 8, 12, 12, 0, 0, DateTimeKind.Utc)));

            quest.QuestOccurrences.Select(o => o.PeriodStart).Should().Equal(
                new DateOnly(2020, 8, 3),
                new DateOnly(2020, 8, 10));
        }
    }
}
