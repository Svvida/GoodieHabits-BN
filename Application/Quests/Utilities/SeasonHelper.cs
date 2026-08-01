using Domain.Enums;
using Domain.Exceptions;

namespace Application.Quests.Utilities
{
    /// <summary>
    /// Season boundaries as calendar dates. A season is a property of the calendar, not of any
    /// instant, so everything here works in <see cref="DateOnly"/>.
    /// </summary>
    public static class SeasonHelper
    {
        public static (DateOnly start, DateOnly end) GetSeasonDateRange(SeasonEnum season, DateOnly today)
        {
            int year = today.Year;

            // Handle Winter separately because it spans two years
            if (season == SeasonEnum.Winter)
            {
                if (today.Month < 3 || (today.Month == 3 && today.Day <= 20))
                    return (new DateOnly(year - 1, 12, 21), new DateOnly(year, 03, 20));

                return (new DateOnly(year, 12, 21), new DateOnly(year + 1, 03, 20));
            }

            return season switch
            {
                SeasonEnum.Spring => (new DateOnly(year, 3, 21), new DateOnly(year, 6, 20)),
                SeasonEnum.Summer => (new DateOnly(year, 6, 21), new DateOnly(year, 9, 22)),
                SeasonEnum.Autumn => (new DateOnly(year, 9, 23), new DateOnly(year, 12, 20)),
                _ => throw new InvalidArgumentException($"Invalid season: {season}")
            };
        }

        public static bool IsDateWithinSeason(DateOnly? startDate, DateOnly? endDate, SeasonEnum season, DateOnly today)
        {
            var seasonRange = GetSeasonDateRange(season, today);
            var earliestAllowed = seasonRange.start.AddDays(-1);

            if (startDate.HasValue && (startDate < earliestAllowed || startDate > seasonRange.end))
                return false;
            if (endDate.HasValue && (endDate > seasonRange.end || endDate < earliestAllowed))
                return false;

            return true;
        }

        public static SeasonEnum GetCurrentSeason(DateOnly today)
        {
            var seasons = new Dictionary<SeasonEnum, (DateOnly start, DateOnly end)>
            {
                {SeasonEnum.Winter, GetSeasonDateRange(SeasonEnum.Winter, today)},
                {SeasonEnum.Spring, GetSeasonDateRange(SeasonEnum.Spring, today)},
                {SeasonEnum.Summer, GetSeasonDateRange(SeasonEnum.Summer, today)},
                {SeasonEnum.Autumn, GetSeasonDateRange(SeasonEnum.Autumn, today)}
            };

            foreach (var season in seasons)
            {
                if (today >= season.Value.start && today <= season.Value.end)
                    return season.Key;
            }

            throw new InvalidArgumentException("Current season not found. This should never happen.");
        }
    }
}
