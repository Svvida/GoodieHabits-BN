using Domain.Enums;
using Domain.Exceptions;

namespace Application.Quests.Utilities
{
    public static class SeasonHelper
    {
        public static (DateTime start, DateTime end) GetSeasonDateRange(SeasonEnum season, DateTime utcNow)
        {
            int year = utcNow.Year;

            // Handle Winter separately because it spans two years
            if (season == SeasonEnum.Winter)
            {
                if (utcNow.Month <= 3 && utcNow.Day <= 20)
                    return (new DateTime(year - 1, 12, 21), new DateTime(year, 03, 20));

                return (new DateTime(year, 12, 21), new DateTime(year + 1, 03, 20));
            }

            return season switch
            {
                SeasonEnum.Spring => (new DateTime(year, 3, 21), new DateTime(year, 6, 20)),
                SeasonEnum.Summer => (new DateTime(year, 6, 21), new DateTime(year, 9, 22)),
                SeasonEnum.Autumn => (new DateTime(year, 9, 23), new DateTime(year, 12, 20)),
                _ => throw new InvalidArgumentException($"Invalid season: {season}")
            };
        }

        public static bool IsDateWithinSeason(DateTime? startDate, DateTime? endDate, SeasonEnum season, DateTime utcNow)
        {
            var seasonRange = GetSeasonDateRange(season, utcNow);

            if (startDate.HasValue && (startDate < seasonRange.start.AddDays(-1) || startDate > seasonRange.end))
                return false;
            if (endDate.HasValue && (endDate > seasonRange.end || endDate < seasonRange.start.AddDays(-1)))
                return false;

            return true;
        }

        public static SeasonEnum GetCurrentSeason(DateTime utcNow)
        {
            var today = utcNow.Date;

            var seasons = new Dictionary<SeasonEnum, (DateTime start, DateTime end)>
            {
                {SeasonEnum.Winter, GetSeasonDateRange(SeasonEnum.Winter,utcNow)},
                {SeasonEnum.Spring, GetSeasonDateRange(SeasonEnum.Spring, utcNow)},
                {SeasonEnum.Summer, GetSeasonDateRange(SeasonEnum.Summer, utcNow)},
                {SeasonEnum.Autumn, GetSeasonDateRange(SeasonEnum.Autumn, utcNow)}
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
