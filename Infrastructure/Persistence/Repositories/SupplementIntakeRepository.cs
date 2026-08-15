using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SupplementIntakeRepository(AppDbContext context)
        : BaseRepository<SupplementIntake>(context), ISupplementIntakeRepository
    {
        public async Task<IReadOnlyList<SupplementIntake>> GetForDayAsync(
            int userProfileId, DateOnly day, CancellationToken cancellationToken = default)
        {
            return await _context.SupplementIntakes
                .AsNoTracking()
                .Where(i => i.UserProfileId == userProfileId && i.TakenOn == day)
                .OrderBy(i => i.TakenAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SupplementIntake>> GetForPeriodAsync(
            int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            return await _context.SupplementIntakes
                .AsNoTracking()
                .Where(i => i.UserProfileId == userProfileId && i.TakenOn >= from && i.TakenOn <= to)
                .OrderBy(i => i.TakenOn)
                .ThenBy(i => i.TakenAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SupplementIntake?> GetBySlotAndDayAsync(
            int scheduleSlotId, DateOnly day, CancellationToken cancellationToken = default)
        {
            // Tracked: the toggle handler either updates or removes this row, making the endpoint idempotent.
            return await _context.SupplementIntakes
                .FirstOrDefaultAsync(
                    i => i.ScheduleSlotId == scheduleSlotId && i.TakenOn == day, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SupplementIntake>> GetForSlotAsync(
            int scheduleSlotId, CancellationToken cancellationToken = default)
        {
            // Tracked: the slot delete handler clears ScheduleSlotId on these in the same unit of work.
            return await _context.SupplementIntakes
                .Where(i => i.ScheduleSlotId == scheduleSlotId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
