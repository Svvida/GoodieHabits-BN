using Domain.Exceptions;
using NodaTime;

namespace Domain.ValueObjects
{
    /// <summary>
    /// Projects UTC instants onto a user's local calendar. The single definition of "what day is it for
    /// this user" — quest periods are calendar facts, so every period boundary is derived through here.
    /// </summary>
    public static class LocalCalendar
    {
        public static DateOnly LocalDateOn(string timeZoneId, DateTime instantUtc)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)
                ?? throw new InvalidArgumentException($"Unknown time zone '{timeZoneId}'.");

            var localDate = Instant.FromDateTimeUtc(DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc))
                .InZone(zone)
                .Date;

            return new DateOnly(localDate.Year, localDate.Month, localDate.Day);
        }
    }

    /// <summary>
    /// Just enough of a profile to decide whether the daily maintenance pass needs to run, without loading
    /// the user's quests to find out.
    /// </summary>
    public readonly record struct MaintenanceWatermark(string? TimeZone, DateOnly? MaintainedThrough);
}
