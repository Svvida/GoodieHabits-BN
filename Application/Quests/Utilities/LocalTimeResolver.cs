using NodaTime;

namespace Application.Quests.Utilities
{
    /// <summary>
    /// The user's wall-clock time at a given instant. Snapshotted onto each completion so the hour-of-day
    /// breakdown keeps reporting when the user actually did things, even after they travel and their
    /// profile timezone is rewritten on the next token refresh.
    /// </summary>
    public static class LocalTimeResolver
    {
        public static TimeOnly? Resolve(string? timeZoneId, DateTime instantUtc)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return null;

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (zone is null)
                return null;

            var local = Instant.FromDateTimeUtc(DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc))
                .InZone(zone)
                .LocalDateTime;

            return new TimeOnly(local.Hour, local.Minute, local.Second);
        }
    }
}
