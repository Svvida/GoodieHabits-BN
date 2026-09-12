using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Services
{
    /// <summary>
    /// Runs the once-a-local-day housekeeping for one user: materialize the quest periods that have come
    /// due, refresh the statistics cache, expire goals, recompute the "currently completed" counter.
    /// </summary>
    public interface IUserMaintenanceService
    {
        /// <summary>
        /// Brings the user's derived state up to their local today. Cheap and idempotent after the first
        /// call of the day — the watermark short-circuits it without touching the database.
        /// </summary>
        Task<bool> EnsureMaintainedAsync(int userProfileId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The replacement for the startup-only background jobs.
    /// <para>
    /// The API runs on shared IIS with an app pool that shuts down after fifteen idle minutes, so no timer
    /// can be relied on to fire. Making this request-driven, per-user and idempotent removes the dependency
    /// on a scheduler entirely: the work happens on the first request of the user's day, which is exactly
    /// when it matters. The startup tasks remain as a free safety net rather than as the mechanism.
    /// </para>
    /// <para>
    /// It is a deliberate exception to "reads never write" — a read path triggers it — and it is committed
    /// separately from whatever the caller is doing.
    /// </para>
    /// </summary>
    public class UserMaintenanceService(
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<UserMaintenanceService> logger) : IUserMaintenanceService
    {
        public async Task<bool> EnsureMaintainedAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();

            // Cheap pre-check: avoids loading every quest on all but the first request of the day. The
            // watermark is a local date, so it is compared against the user's own today, not the server's.
            var watermark = await unitOfWork.UserProfiles
                .GetMaintenanceWatermarkAsync(userProfileId, cancellationToken)
                .ConfigureAwait(false);

            if (watermark.TimeZone is null)
                return false;

            var today = LocalCalendar.LocalDateOn(watermark.TimeZone, nowUtc);
            if (watermark.MaintainedThrough == today)
                return false;

            var profile = await unitOfWork.UserProfiles
                .GetProfileForMaintenanceAsync(userProfileId, cancellationToken)
                .ConfigureAwait(false);

            if (profile is null)
                return false;

            int generated = profile.RunDailyMaintenance(nowUtc);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogDebug("Maintenance ran for profile {UserProfileId}: {Generated} period(s) generated.",
                userProfileId, generated);

            return true;
        }
    }
}
