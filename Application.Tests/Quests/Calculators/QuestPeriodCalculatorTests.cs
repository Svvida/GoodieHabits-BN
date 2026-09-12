using Domain.Calculators;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    /// <summary>
    /// The scheduling rules the whole quest module rests on. Everything here is pure calendar arithmetic, so
    /// these run without a database, a clock or a user profile.
    /// <para>
    /// Reference dates: 2026-09-07 is a Monday, 2026-09-12 a Saturday, 2026-09-01 a Tuesday.
    /// </para>
    /// </summary>
    public class QuestPeriodCalculatorTests
    {
        private static readonly DateOnly Sep1 = new(2026, 9, 1);   // Tuesday
        private static readonly DateOnly Sep7 = new(2026, 9, 7);   // Monday
        private static readonly DateOnly Sep30 = new(2026, 9, 30);

        private static QuestScheduleBounds Bounds(DateOnly anchor, DateOnly? from = null, DateOnly? to = null)
            => new(anchor, from ?? anchor, to);

        private static IReadOnlyList<QuestPeriodWindow> Periods(
            QuestSchedule schedule,
            QuestScheduleBounds bounds,
            DateOnly from,
            DateOnly to,
            DayOfWeek weekStartsOn = DayOfWeek.Monday)
            => QuestPeriodCalculator.PeriodsBetween(schedule, bounds, weekStartsOn, from, to);

        // ───────────────────────────────── Day ─────────────────────────────────

        [Fact]
        public void Daily_ShouldProduceOnePeriodPerDay()
        {
            var periods = Periods(QuestSchedule.Daily(), Bounds(Sep1), Sep1, Sep1.AddDays(4));

            periods.Should().HaveCount(5);
            periods.Should().OnlyContain(p => p.Start == p.End && !p.IsPartial);
            periods[0].Start.Should().Be(Sep1);
            periods[4].Start.Should().Be(new DateOnly(2026, 9, 5));
        }

        [Fact]
        public void Daily_WithWeekdays_ShouldProduceOnlyThoseDays()
        {
            var weekdays = WeekdayFlags.Monday | WeekdayFlags.Wednesday | WeekdayFlags.Friday;

            var periods = Periods(QuestSchedule.Daily(weekdays: weekdays), Bounds(Sep7), Sep7, Sep7.AddDays(6));

            periods.Select(p => p.Start.DayOfWeek).Should().Equal(
                DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday);
        }

        [Fact]
        public void Daily_WithInterval_ShouldSkipDaysAndStayAnchored()
        {
            // "Every other day", counted from the anchor — not from whenever generation happens to run.
            var periods = Periods(QuestSchedule.Daily(interval: 2), Bounds(Sep1), Sep1, Sep1.AddDays(5));

            periods.Select(p => p.Start).Should().Equal(
                Sep1, Sep1.AddDays(2), Sep1.AddDays(4));
        }

        [Fact]
        public void Daily_WithInterval_ShouldStayAnchoredWhenGeneratingALaterRange()
        {
            var schedule = QuestSchedule.Daily(interval: 3);
            var bounds = Bounds(Sep1);

            // Resuming generation mid-series must land on the same days the first pass would have.
            var periods = Periods(schedule, bounds, Sep1.AddDays(10), Sep1.AddDays(15));

            periods.Select(p => p.Start).Should().Equal(
                Sep1.AddDays(12), Sep1.AddDays(15));
        }

        [Fact]
        public void Daily_AllSevenWeekdays_ShouldEqualPlainDaily()
        {
            QuestSchedule.Daily(weekdays: WeekdayFlags.All).Should().Be(QuestSchedule.Daily());
        }

        // ───────────────────────────────── Week ────────────────────────────────

        [Fact]
        public void Weekly_ShouldProduceMondayToSundayPeriods()
        {
            var periods = Periods(QuestSchedule.Weekly(), Bounds(Sep7), Sep7, Sep7.AddDays(13));

            periods.Should().HaveCount(2);
            periods[0].Start.Should().Be(Sep7);
            periods[0].End.Should().Be(new DateOnly(2026, 9, 13));  // Sunday
            periods[1].Start.Should().Be(new DateOnly(2026, 9, 14));
        }

        [Fact]
        public void Weekly_ShouldHonourASundayWeekStart()
        {
            var periods = Periods(QuestSchedule.Weekly(), Bounds(Sep7), Sep7, Sep7.AddDays(6), DayOfWeek.Sunday);

            // The Monday anchor sits inside the week that began the previous Sunday.
            periods[0].FullStart.Should().Be(new DateOnly(2026, 9, 6));
            periods[0].FullEnd.Should().Be(new DateOnly(2026, 9, 12));
        }

        [Fact]
        public void Weekly_WithInterval_ShouldEmitEveryOtherWeek()
        {
            var periods = Periods(QuestSchedule.Weekly(interval: 2), Bounds(Sep7), Sep7, Sep7.AddDays(28));

            periods.Select(p => p.Start).Should().Equal(
                Sep7, Sep7.AddDays(14), Sep7.AddDays(28));

            // Returned whole: the last period runs past the requested range.
            periods.Last().End.Should().Be(Sep7.AddDays(34));
        }

        // ──────────────────────────────── Month ────────────────────────────────

        [Fact]
        public void Monthly_WithoutAWindow_ShouldCoverWholeMonths()
        {
            var periods = Periods(QuestSchedule.Monthly(), Bounds(Sep1), Sep1, new DateOnly(2026, 10, 31));

            periods.Should().HaveCount(2);
            periods[0].Start.Should().Be(Sep1);
            periods[0].End.Should().Be(Sep30);
            periods[1].End.Should().Be(new DateOnly(2026, 10, 31));
        }

        [Fact]
        public void Monthly_WithAWindow_ShouldCoverOnlyThoseDays()
        {
            var schedule = QuestSchedule.Monthly(windowStartDay: 1, windowEndDay: 5);

            var periods = Periods(schedule, Bounds(Sep1), Sep1, new DateOnly(2026, 10, 31));

            periods[0].Start.Should().Be(Sep1);
            periods[0].End.Should().Be(new DateOnly(2026, 9, 5));
            periods[1].Start.Should().Be(new DateOnly(2026, 10, 1));
        }

        [Fact]
        public void Monthly_WindowDayBeyondTheMonthLength_ShouldClamp()
        {
            var schedule = QuestSchedule.Monthly(windowStartDay: 30, windowEndDay: 31);
            var bounds = Bounds(new DateOnly(2026, 2, 1));

            var periods = Periods(schedule, bounds, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

            // February still gets a period rather than being silently skipped.
            periods.Should().HaveCount(1);
            periods[0].Start.Should().Be(new DateOnly(2026, 2, 28));
            periods[0].End.Should().Be(new DateOnly(2026, 2, 28));
        }

        // ───────────────────────────────── Year ────────────────────────────────

        [Fact]
        public void Yearly_WithAWrappingWindow_ShouldSpanTheYearBoundary()
        {
            // The old Seasonal quest: winter, 21 December to 20 March.
            var schedule = QuestSchedule.Yearly(windowStart: 1221, windowEnd: 0320);
            var bounds = Bounds(new DateOnly(2026, 12, 21));

            var periods = Periods(schedule, bounds, new DateOnly(2026, 12, 1), new DateOnly(2027, 4, 1));

            periods.Should().HaveCount(1);
            periods[0].Start.Should().Be(new DateOnly(2026, 12, 21));
            periods[0].End.Should().Be(new DateOnly(2027, 3, 20));
        }

        [Fact]
        public void Yearly_WithAWrappingWindow_ShouldRecurEveryYear()
        {
            var schedule = QuestSchedule.Yearly(windowStart: 1221, windowEnd: 0320);
            var bounds = Bounds(new DateOnly(2026, 12, 21));

            var periods = Periods(schedule, bounds, new DateOnly(2026, 12, 1), new DateOnly(2028, 4, 1));

            // The seasonal quest that used to stay completed for ever now comes back next winter.
            periods.Should().HaveCount(2);
            periods[1].Start.Should().Be(new DateOnly(2027, 12, 21));
        }

        [Fact]
        public void Yearly_WithoutAWindow_ShouldCoverTheWholeYear()
        {
            var bounds = Bounds(new DateOnly(2026, 1, 1));

            var periods = Periods(QuestSchedule.Yearly(), bounds, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

            periods.Should().HaveCount(1);
            periods[0].End.Should().Be(new DateOnly(2026, 12, 31));
        }

        // ───────────────────────────────── None ────────────────────────────────

        [Fact]
        public void OneTime_ShouldProduceASinglePeriodCoveringTheActiveRange()
        {
            var bounds = Bounds(Sep1, Sep1, Sep30);

            var periods = Periods(QuestSchedule.OneTime(), bounds, Sep1, Sep30);

            periods.Should().HaveCount(1);
            periods[0].Start.Should().Be(Sep1);
            periods[0].End.Should().Be(Sep30);
            // Crucially not treated as partial — otherwise its target would be prorated away.
            periods[0].IsPartial.Should().BeFalse();
        }

        // ────────────────────── clipping and proration ─────────────────────────

        [Fact]
        public void AQuestStartingMidWeek_ShouldGetAClippedPartialFirstPeriod()
        {
            var sep12 = new DateOnly(2026, 9, 12);  // Saturday
            var bounds = Bounds(sep12);

            var periods = Periods(QuestSchedule.Weekly(), bounds, sep12, sep12.AddDays(8));

            periods[0].Start.Should().Be(sep12);
            periods[0].End.Should().Be(new DateOnly(2026, 9, 13));
            periods[0].FullStart.Should().Be(Sep7);
            periods[0].FullDays.Should().Be(7);
            periods[0].Days.Should().Be(2);
            periods[0].IsPartial.Should().BeTrue();

            // The next week is whole again.
            periods[1].IsPartial.Should().BeFalse();
        }

        [Fact]
        public void AnEndDate_ShouldClipTheFinalPeriod()
        {
            var bounds = Bounds(Sep7, Sep7, new DateOnly(2026, 9, 9));

            var periods = Periods(QuestSchedule.Weekly(), bounds, Sep7, Sep30);

            periods.Should().HaveCount(1);
            periods[0].End.Should().Be(new DateOnly(2026, 9, 9));
            periods[0].IsPartial.Should().BeTrue();
        }

        [Fact]
        public void PeriodsBetween_ShouldNeverGenerateOutsideTheActiveRange()
        {
            var bounds = Bounds(Sep7, Sep7, new DateOnly(2026, 9, 10));

            var periods = Periods(QuestSchedule.Daily(), bounds, Sep1, Sep30);

            periods.Should().HaveCount(4);
            periods.First().Start.Should().Be(Sep7);
            periods.Last().Start.Should().Be(new DateOnly(2026, 9, 10));
        }

        [Fact]
        public void PeriodsBetween_ShouldReturnMultiDayPeriodsWholeWhenTheyStraddleTheRangeEdge()
        {
            var bounds = Bounds(Sep1);

            // Asking only about the 20th must still return September's whole period.
            var periods = Periods(QuestSchedule.Monthly(), bounds, new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 20));

            periods.Should().HaveCount(1);
            periods[0].End.Should().Be(Sep30);
        }

        // ────────────────────────────── PeriodCovering ─────────────────────────

        [Fact]
        public void PeriodCovering_ShouldFindThePeriodADateFallsIn()
        {
            var period = QuestPeriodCalculator.PeriodCovering(
                QuestSchedule.Weekly(), Bounds(Sep7), DayOfWeek.Monday, new DateOnly(2026, 9, 10));

            period.Should().NotBeNull();
            period!.Value.Start.Should().Be(Sep7);
        }

        [Fact]
        public void PeriodCovering_ShouldReturnNullOnAnUnscheduledWeekday()
        {
            var schedule = QuestSchedule.Daily(weekdays: WeekdayFlags.Monday | WeekdayFlags.Wednesday);

            var period = QuestPeriodCalculator.PeriodCovering(
                schedule, Bounds(Sep7), DayOfWeek.Monday, new DateOnly(2026, 9, 8));  // Tuesday

            period.Should().BeNull();
        }

        [Fact]
        public void PeriodCovering_ShouldReturnNullOutsideTheActiveRange()
        {
            var bounds = Bounds(Sep7, Sep7, new DateOnly(2026, 9, 9));

            QuestPeriodCalculator.PeriodCovering(QuestSchedule.Daily(), bounds, DayOfWeek.Monday, Sep1)
                .Should().BeNull();

            QuestPeriodCalculator.PeriodCovering(QuestSchedule.Daily(), bounds, DayOfWeek.Monday, Sep30)
                .Should().BeNull();
        }

        [Fact]
        public void StartOfWeek_ShouldHonourTheConfiguredWeekStart()
        {
            var saturday = new DateOnly(2026, 9, 12);

            QuestPeriodCalculator.StartOfWeek(saturday, DayOfWeek.Monday).Should().Be(Sep7);
            QuestPeriodCalculator.StartOfWeek(saturday, DayOfWeek.Sunday).Should().Be(new DateOnly(2026, 9, 6));
        }
    }
}
