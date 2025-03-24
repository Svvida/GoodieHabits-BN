using Domain.Enum;
using Domain.Exceptions;

namespace Application.Helpers
{
    public static class SeasonHelper
    {
        public static (DateTime start, DateTime end) GetSeasonDateRange(SeasonEnum season)
        {
            int year = DateTime.UtcNow.Year;
            DateTime today = DateTime.UtcNow.Date;

            // Handle Winter separately because it spans two years
            if (season == SeasonEnum.Winter)
            {
                if (today.Month <= 3 && today.Day <= 20)
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

        public static bool IsDateWithinSeason(DateTime? startDate, DateTime? endDate, SeasonEnum season)
        {
            var seasonRange = GetSeasonDateRange(season);

            if (startDate.HasValue && (startDate < seasonRange.start.AddDays(-1) || startDate > seasonRange.end))
                return false;
            if (endDate.HasValue && (endDate > seasonRange.end || endDate < seasonRange.start.AddDays(-1)))
                return false;

            return true;
        }

        public static SeasonEnum GetCurrentSeason()
        {
            var today = DateTime.UtcNow.Date;

            var seasons = new Dictionary<SeasonEnum, (DateTime start, DateTime end)>
            {
                {SeasonEnum.Winter, GetSeasonDateRange(SeasonEnum.Winter)},
                {SeasonEnum.Spring, GetSeasonDateRange(SeasonEnum.Spring)},
                {SeasonEnum.Summer, GetSeasonDateRange(SeasonEnum.Summer)},
                {SeasonEnum.Autumn, GetSeasonDateRange(SeasonEnum.Autumn)}
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
