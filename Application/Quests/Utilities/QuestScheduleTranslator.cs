using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Quests.Utilities
{
    /// <summary>
    /// Turns the wire shapes into the domain value objects. Field-level rules (is this a real enum name, is
    /// the interval in range) belong to the validators; everything here either parses or delegates to the
    /// value object's own invariants, so there is exactly one definition of a legal schedule.
    /// </summary>
    public static class QuestScheduleTranslator
    {
        public static QuestSchedule ToSchedule(QuestScheduleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var unit = ParseUnit(request.Unit);

            return unit switch
            {
                PeriodUnitEnum.None => QuestSchedule.OneTime(),
                PeriodUnitEnum.Day => QuestSchedule.Daily(request.Interval, ParseWeekdays(request.Weekdays)),
                PeriodUnitEnum.Week => QuestSchedule.Weekly(request.Interval),
                PeriodUnitEnum.Month => QuestSchedule.Monthly(request.Interval, request.MonthWindowStartDay, request.MonthWindowEndDay),
                PeriodUnitEnum.Year => QuestSchedule.Yearly(request.Interval, request.YearWindowStart, request.YearWindowEnd),
                _ => throw new InvalidArgumentException($"Unsupported schedule unit '{request.Unit}'.")
            };
        }

        public static QuestTarget ToTarget(QuestTargetRequest? request)
        {
            if (request is null)
                return QuestTarget.Once();

            var mode = ParseMode(request.Mode);

            if (mode == TargetModeEnum.AtMost)
                throw new InvalidArgumentException("Limit habits ('AtMost') are not supported yet.");

            return QuestTarget.Create(request.Amount, request.Unit, mode, request.MaxCompletionsPerDay);
        }

        public static PeriodUnitEnum ParseUnit(string? unit)
        {
            if (!Enum.TryParse<PeriodUnitEnum>(unit, ignoreCase: true, out var parsed))
                throw new InvalidArgumentException($"'{unit}' is not a valid schedule unit.");

            return parsed;
        }

        private static TargetModeEnum ParseMode(string? mode)
        {
            if (!Enum.TryParse<TargetModeEnum>(mode, ignoreCase: true, out var parsed))
                throw new InvalidArgumentException($"'{mode}' is not a valid target mode.");

            return parsed;
        }

        public static WeekdayFlags? ParseWeekdays(IEnumerable<string>? weekdays)
        {
            if (weekdays is null)
                return null;

            var names = weekdays.ToList();
            if (names.Count == 0)
                return null;

            var flags = WeekdayFlags.None;

            foreach (var name in names)
            {
                if (!Enum.TryParse<WeekdayEnum>(name, ignoreCase: true, out var weekday))
                    throw new InvalidArgumentException($"'{name}' is not a valid weekday.");

                flags |= weekday.ToFlag();
            }

            return flags;
        }
    }
}
