using Application.Quests.Queries.GetHabitsOverview;
using Application.Quests.Services;
using Domain.Models;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Quests.Queries.GetHabitsOverview
{
    public class GetHabitsOverviewQueryHandlerTests : TestBase<GetHabitsOverviewQueryHandler>
    {
        // TestBase freezes the clock at 2023-10-26; the profile below is Etc/UTC so that is also "today".
        private static readonly DateOnly From = new(2023, 10, 1);
        private static readonly DateOnly To = new(2023, 10, 26);

        private GetHabitsOverviewQueryHandler CreateHandler() =>
            new(_unitOfWork, NoOpMaintenance(), _clockMock.Object);

        /// <summary>
        /// The handler triggers the daily maintenance pass; these tests seed periods directly, so it has
        /// nothing to do and is stubbed out rather than exercised here.
        /// </summary>
        private static IUserMaintenanceService NoOpMaintenance()
        {
            var mock = new Mock<IUserMaintenanceService>();
            mock.Setup(m => m.EnsureMaintainedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            return mock.Object;
        }

        [Fact]
        public async Task Handle_ShouldReturnOneEntryPerQuest_NotPerOccurrence()
        {
            var profile = await AddProfileAsync();
            var quest = AddQuest(profile, "Read a book");
            AddPeriod(quest, new DateOnly(2023, 10, 20));
            AddPeriod(quest, new DateOnly(2023, 10, 21));
            AddPeriod(quest, new DateOnly(2023, 10, 22));
            await _context.SaveChangesAsync();

            var result = await CreateHandler().Handle(
                new GetHabitsOverviewQuery(profile.Id, From, To),
                CancellationToken.None);

            // The occurrences are read AsNoTracking, so each row carries its own Quest instance. Grouping on
            // the navigation property (reference equality) would return three entries for this one quest.
            result.Quests.Should().ContainSingle();
            result.Quests[0].QuestId.Should().Be(quest.Id);
            result.Quests[0].Title.Should().Be("Read a book");
            result.Quests[0].Summary.TotalPeriods.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ShouldKeepDistinctQuestsSeparate()
        {
            var profile = await AddProfileAsync();
            var first = AddQuest(profile, "Read a book");
            var second = AddQuest(profile, "Stretch");
            AddPeriod(first, new DateOnly(2023, 10, 20));
            AddPeriod(first, new DateOnly(2023, 10, 21));
            AddPeriod(second, new DateOnly(2023, 10, 20));
            await _context.SaveChangesAsync();

            var result = await CreateHandler().Handle(
                new GetHabitsOverviewQuery(profile.Id, From, To),
                CancellationToken.None);

            result.Quests.Should().HaveCount(2);
            result.Quests.Select(q => q.QuestId).Should().BeEquivalentTo([first.Id, second.Id]);
            result.Quests.Single(q => q.QuestId == first.Id).Summary.TotalPeriods.Should().Be(2);
            result.Quests.Single(q => q.QuestId == second.Id).Summary.TotalPeriods.Should().Be(1);
        }

        private async Task<UserProfile> AddProfileAsync()
        {
            var account = await AddAccountAsync("tester@example.com", "hash", "tester");
            return account.Profile;
        }

        private Quest AddQuest(UserProfile profile, string title)
        {
            var quest = Quest.Create(
                title: title,
                userProfile: profile,
                schedule: QuestSchedule.Daily(),
                target: QuestTarget.Once(),
                nowUtc: _fixedTestInstant.ToDateTimeUtc());

            _context.Quests.Add(quest);
            return quest;
        }

        private static void AddPeriod(Quest quest, DateOnly day) =>
            quest.QuestOccurrences.Add(QuestOccurrence.Create(quest, day, day, 1m));
    }
}
