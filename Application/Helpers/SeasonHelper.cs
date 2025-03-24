using Domain.Enum;

namespace Application.Helpers
{
    public static class SeasonHelper
    {
        public static (DateTime start, DateTime end)? GetSeasonDateRange(SeasonEnum season)
        {
            int year = DateTime.UtcNow.Year;

            return season switch
            {
                SeasonEnum.Winter => (new DateTime(year, 12, 21), new DateTime(year + 1, 03, 20)),
                SeasonEnum.Spring => (new DateTime(year, 3, 21), new DateTime(year, 6, 20)),
                SeasonEnum.Summer => (new DateTime(year, 6, 21), new DateTime(year, 9, 22)),
                SeasonEnum.Autumn => (new DateTime(year, 9, 23), new DateTime(year, 12, 20)),
                _ => null
            };
        }

        public static bool IsDateWithinSeason(DateTime? startDate, DateTime? endDate, SeasonEnum season)
        {
            var seasonRange = GetSeasonDateRange(season);
            if (seasonRange is null)
                return false;

            if (startDate.HasValue && (startDate < seasonRange.Value.start.AddDays(-1) || startDate > seasonRange.Value.end))
                return false;
            if (endDate.HasValue && (endDate > seasonRange.Value.end || endDate < seasonRange.Value.start.AddDays(-1)))
                return false;

            return true;
        }
    }
}
