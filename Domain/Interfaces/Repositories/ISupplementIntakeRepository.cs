using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface ISupplementIntakeRepository : IBaseRepository<SupplementIntake>
    {
        /// <summary>Everything logged on one calendar day — the state half of the checklist.</summary>
        Task<IReadOnlyList<SupplementIntake>> GetForDayAsync(
            int userProfileId, DateOnly day, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SupplementIntake>> GetForPeriodAsync(
            int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        /// <summary>
        /// The single row a slot may have on a given day, tracked. This is what makes the toggle idempotent:
        /// the handler upserts on it rather than blindly inserting, and the filtered unique index on
        /// <c>(ScheduleSlotId, TakenOn)</c> is the backstop when two taps race.
        /// </summary>
        Task<SupplementIntake?> GetBySlotAndDayAsync(
            int scheduleSlotId, DateOnly day, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked intakes for a slot, so deleting the slot can null their FK instead of destroying the record
        /// of doses actually taken. They become ordinary ad-hoc intakes.
        /// </summary>
        Task<IReadOnlyList<SupplementIntake>> GetForSlotAsync(
            int scheduleSlotId, CancellationToken cancellationToken = default);
    }
}
