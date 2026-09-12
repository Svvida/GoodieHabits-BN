using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Application.Tests.Quests
{
    /// <summary>
    /// The completion flow: targets above one, per-day caps, idempotency, off-schedule taps, and the reward
    /// rules. These are the behaviours the old boolean <c>IsCompleted</c> could not express at all.
    /// </summary>
    public class QuestCompletionTests
    {
        private static readonly DateTime NowUtc = new(2020, 8, 5, 9, 0, 0, DateTimeKind.Utc);
        private static readonly DateOnly Today = new(2020, 8, 5);

        // ─────────────────────────── targets above one ───────────────────────────

        [Fact]
        public void ATargetOfTwo_ShouldNeedTwoCompletions()
        {
            // "Brush your teeth twice a day" — one quest, not two.
            var quest = QuestTestFactory.Daily(target: 2m);
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today);

            quest.IsCompletedOn(Today).Should().BeFalse("one of two brushings is not a finished day");
            quest.CurrentPeriod(Today)!.Progress.Should().Be(1m);

            quest.AddCompletion(NowUtc, Today);

            quest.IsCompletedOn(Today).Should().BeTrue();
        }

        [Fact]
        public void AWeeklyTarget_ShouldBeSatisfiedOnAnyDays()
        {
            // "Exercise at least twice a week" — no fixed weekdays, which the old model could not express.
            var quest = QuestTestFactory.TimesPerWeek(2, startDate: new DateOnly(2020, 8, 3));
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));
            quest.AddCompletion(NowUtc, Today, completedOn: Today);

            quest.IsCompletedOn(Today).Should().BeTrue();
        }

        [Fact]
        public void GoingOverTheTarget_ShouldBeRecordedButEarnNothingExtra()
        {
            // The three-workout week on a "twice a week" habit: counted, visible, not double-paid.
            var quest = QuestTestFactory.TimesPerWeek(2, startDate: new DateOnly(2020, 8, 3));
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 3));
            var completing = quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));
            var extra = quest.AddCompletion(NowUtc, Today, completedOn: Today);

            completing.Reward.Xp.Should().BeGreaterThan(0);
            extra.PeriodCompleted.Should().BeFalse();
            extra.Reward.Should().Be(Domain.ValueObjects.QuestReward.None);

            quest.CurrentPeriod(Today)!.Progress.Should().Be(3m);
            quest.Completions.Should().HaveCount(3);
        }

        [Fact]
        public void MaxCompletionsPerDay_ShouldStopAWeeklyTargetBeingFinishedInOneDay()
        {
            var quest = QuestTestFactory.TimesPerWeek(2, startDate: new DateOnly(2020, 8, 3), maxPerDay: 1);
            quest.InitializePeriods(Today);
            quest.AddCompletion(NowUtc, Today);

            var act = () => quest.AddCompletion(NowUtc, Today);

            act.Should().Throw<QuestCompletionException>().WithMessage("*at most 1 completion*");
        }

        // ───────────────────────────── idempotency ──────────────────────────────

        [Fact]
        public void ARepeatedClientRequestId_ShouldNotRecordTwice()
        {
            // Matters far more now that a second tap is legitimate: without this an offline retry would
            // silently finish a "twice a day" habit.
            var quest = QuestTestFactory.Daily(target: 2m);
            quest.InitializePeriods(Today);
            var requestId = Guid.NewGuid();

            quest.AddCompletion(NowUtc, Today, clientRequestId: requestId);
            var replay = quest.AddCompletion(NowUtc, Today, clientRequestId: requestId);

            replay.WasAlreadyRecorded.Should().BeTrue();
            quest.Completions.Should().ContainSingle();
            quest.CurrentPeriod(Today)!.Progress.Should().Be(1m);
        }

        // ──────────────────────── catch-up / backfilling ─────────────────────────

        [Fact]
        public void BackfillingWithinTheGraceWindow_ShouldRepairTheMissedPeriodAndItsStreak()
        {
            var quest = QuestTestFactory.Daily(startDate: new DateOnly(2020, 8, 1));
            quest.InitializePeriods(Today);

            // Older history is seeded straight onto the periods: replaying it through AddCompletion would
            // (correctly) be refused, since those days are outside the catch-up window.
            foreach (var day in new[] { 1, 2, 3 })
            {
                quest.QuestOccurrences
                    .Single(p => p.PeriodStart == new DateOnly(2020, 8, day))
                    .ApplyProgress(1m, NowUtc, Today);
            }

            quest.AddCompletion(NowUtc, Today, completedOn: Today);
            quest.RecalculateStatistics(Today);
            quest.Statistics!.CurrentStreak.Should().Be(1, "the 4th broke the run");

            quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));

            var repaired = quest.QuestOccurrences.Single(p => p.PeriodStart == new DateOnly(2020, 8, 4));
            repaired.IsCompleted.Should().BeTrue();
            repaired.IsBackfilled.Should().BeTrue("it was recorded after the day had elapsed");
            quest.Statistics!.CurrentStreak.Should().Be(5);
        }

        [Fact]
        public void BackfillingBeyondTheGraceWindow_ShouldBeRejected()
        {
            var quest = QuestTestFactory.Daily(startDate: new DateOnly(2020, 8, 1));
            quest.InitializePeriods(Today);

            var act = () => quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 1));

            act.Should().Throw<QuestCompletionException>().WithMessage("*within the last 2 days*");
        }

        [Fact]
        public void CompletingForAFutureDate_ShouldBeRejected()
        {
            var quest = QuestTestFactory.Daily();
            quest.InitializePeriods(Today);

            var act = () => quest.AddCompletion(NowUtc, Today, completedOn: Today.AddDays(1));

            act.Should().Throw<QuestCompletionException>().WithMessage("*future*");
        }

        // ─────────────────────────── off-schedule taps ───────────────────────────

        [Fact]
        public void ATapOnAnUnscheduledDay_ShouldBeRecordedOffScheduleAndEarnNothing()
        {
            // A bonus workout on a Mon/Wed/Fri habit: worth recording, never a reward, never a period.
            var quest = QuestTestFactory.OnWeekdays(
                [WeekdayEnum.Monday, WeekdayEnum.Wednesday],
                startDate: new DateOnly(2020, 8, 1));
            quest.InitializePeriods(Today);   // 5 Aug 2020 is a Wednesday

            var result = quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));

            result.Period.Should().BeNull();
            result.Completion.IsOffSchedule.Should().BeTrue();
            result.Reward.Should().Be(Domain.ValueObjects.QuestReward.None);
            quest.Completions.Should().ContainSingle();
        }

        // ──────────────────────────────── rewards ────────────────────────────────

        [Fact]
        public void RemovingAndReAddingACompletion_ShouldNotPayTwice()
        {
            // The old model handed out 10 coins on every tap and reclaimed none of them, so toggling
            // complete/uncomplete was an unlimited coin farm.
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.Daily(profile);
            quest.InitializePeriods(Today);

            var first = quest.AddCompletion(NowUtc, Today);
            int coinsAfterFirst = profile.Coins;
            int xpAfterFirst = profile.TotalXp;

            first.Reward.Coins.Should().BeGreaterThan(0);

            quest.RemoveCompletion(first.Completion, NowUtc, Today);
            var second = quest.AddCompletion(NowUtc, Today);

            second.Reward.Should().Be(Domain.ValueObjects.QuestReward.None);
            profile.Coins.Should().Be(coinsAfterFirst);
            profile.TotalXp.Should().Be(xpAfterFirst);
        }

        [Fact]
        public void RemovingACompletion_ShouldNotReclaimTheReward()
        {
            var profile = QuestTestFactory.Profile();
            var quest = QuestTestFactory.Daily(profile);
            quest.InitializePeriods(Today);

            var result = quest.AddCompletion(NowUtc, Today);
            int coins = profile.Coins;

            quest.RemoveCompletion(result.Completion, NowUtc, Today);

            quest.IsCompletedOn(Today).Should().BeFalse();
            profile.Coins.Should().Be(coins, "mistakes happen; rewards are never clawed back");
        }

        [Fact]
        public void ALongerPeriod_ShouldBeWorthMoreThanADailyOne()
        {
            var daily = QuestTestFactory.Daily();
            var weekly = QuestTestFactory.TimesPerWeek(1, startDate: new DateOnly(2020, 8, 3));
            daily.InitializePeriods(Today);
            weekly.InitializePeriods(Today);

            var dailyReward = daily.AddCompletion(NowUtc, Today).Reward;
            var weeklyReward = weekly.AddCompletion(NowUtc, Today).Reward;

            // Otherwise a habit counted in small units out-earns a bigger commitment purely by arithmetic.
            weeklyReward.Xp.Should().BeGreaterThan(dailyReward.Xp);
        }

        // ──────────────────────────────── outcomes ───────────────────────────────

        [Fact]
        public void AnElapsedPeriodWithSomeProgress_ShouldBePartialRatherThanMissed()
        {
            var quest = QuestTestFactory.Daily(target: 2m, startDate: new DateOnly(2020, 8, 4));
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));

            var yesterday = quest.QuestOccurrences.Single(p => p.PeriodStart == new DateOnly(2020, 8, 4));
            yesterday.OutcomeOn(Today).Should().Be(QuestPeriodOutcomeEnum.Partial);

            // Still a failure for rate purposes — it is reported apart only so the UI can show "1 of 2".
            quest.RecalculateStatistics(Today);
            quest.Statistics!.FailureCount.Should().Be(1);
            quest.Statistics!.PartialCount.Should().Be(1);
        }

        [Fact]
        public void AnOpenEndedOneTimeQuest_ShouldNotBreakPeriodGeneration()
        {
            // Its single period runs to DateOnly.MaxValue, so naively asking for "the day after the last
            // period" overflows. 28 rows in production look exactly like this.
            var quest = QuestTestFactory.OneTime(startDate: new DateOnly(2020, 8, 1));
            quest.InitializePeriods(Today);

            quest.QuestOccurrences.Single().PeriodEnd.Should().Be(DateOnly.MaxValue);

            var act = () => quest.GenerateMissingPeriodsOn(Today);

            act.Should().NotThrow();
            act().Should().Be(0);
            quest.QuestOccurrences.Should().ContainSingle();
        }

        [Fact]
        public void AnOpenEndedOneTimeQuest_ShouldStillBeCompletable()
        {
            var quest = QuestTestFactory.OneTime(startDate: new DateOnly(2020, 8, 1));
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today);

            quest.IsCompletedOn(Today).Should().BeTrue();
        }

        // ───────────── what the client needs back to run its own UI ──────────────

        [Fact]
        public void TheCurrentPeriod_ShouldExposeTheIdsNeededToUndoATap()
        {
            // Regression for the hole the FE review found: completion ids were only ever returned by the
            // POST that created them, so undo silently stopped working once the app restarted.
            var quest = QuestTestFactory.Daily(target: 2m);
            quest.InitializePeriods(Today);

            var first = quest.AddCompletion(NowUtc, Today);
            quest.AddCompletion(NowUtc, Today);

            var period = quest.CurrentPeriod(Today)!;

            period.Completions.Should().HaveCount(2);
            period.Completions.Select(c => c.Id).Should().Contain(first.Completion.Id);
        }

        [Fact]
        public void TheCurrentPeriod_ShouldDistinguishTodaysTapsFromEarlierOnesInTheSamePeriod()
        {
            // The other half of the same hole: with a weekly target the client cannot tell whether the
            // recorded progress happened today, so it cannot honour maxCompletionsPerDay after a restart.
            var quest = QuestTestFactory.TimesPerWeek(2, startDate: new DateOnly(2020, 8, 3));
            quest.InitializePeriods(Today);

            quest.AddCompletion(NowUtc, Today, completedOn: new DateOnly(2020, 8, 4));

            var period = quest.CurrentPeriod(Today)!;
            period.Progress.Should().Be(1m);
            period.Completions.Where(c => c.CompletedOn == Today).Should().BeEmpty("yesterday's tap is not today's");

            quest.AddCompletion(NowUtc, Today);

            quest.CurrentPeriod(Today)!.Completions.Where(c => c.CompletedOn == Today).Should().ContainSingle();
        }

        [Fact]
        public void AFinishedOneOff_ShouldNotInviteAnotherTap()
        {
            // Overshooting is a feature for a habit and a misfire for a one-off: "2 / 1" on a quest that
            // happens once means nothing. Raised by the FE review as "who guards this?" — the answer is us.
            var oneOff = QuestTestFactory.OneTime(startDate: new DateOnly(2020, 8, 1));
            oneOff.InitializePeriods(Today);
            oneOff.AddCompletion(NowUtc, Today);

            var habit = QuestTestFactory.Daily();
            habit.InitializePeriods(Today);
            habit.AddCompletion(NowUtc, Today);

            oneOff.IsCompletedOn(Today).Should().BeTrue();
            habit.IsCompletedOn(Today).Should().BeTrue();

            // The domain still accepts an extra tap on the habit; the DTO is what closes the button.
            var act = () => habit.AddCompletion(NowUtc, Today);
            act.Should().NotThrow();
        }

        [Fact]
        public void AQuestStartingMidWeek_ShouldGetAProratedFirstTarget()
        {
            // 8 Aug 2020 is a Saturday: two days of that week remain, so "three times a week" asks for one.
            var quest = QuestTestFactory.TimesPerWeek(3, startDate: new DateOnly(2020, 8, 8));
            var saturday = new DateOnly(2020, 8, 8);

            quest.InitializePeriods(saturday);

            quest.CurrentPeriod(saturday)!.TargetAmount.Should().Be(1m,
                "a partial first period must not open with a guaranteed failure");
        }
    }
}
