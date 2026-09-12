using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class QuestOccurrenceRepository(AppDbContext context) : BaseRepository<QuestOccurrence>(context), IQuestOccurrenceRepository
    {
        public async Task<List<QuestOccurrence>> GetForQuestInRangeAsync(
            int questId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(qo => qo.QuestId == questId)
                // Overlap, not containment: a monthly period straddling the range edge still counts.
                .Where(qo => qo.PeriodStart <= to && qo.PeriodEnd >= from)
                .OrderBy(qo => qo.PeriodStart)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<QuestOccurrence>> GetForUserInRangeAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(qo => qo.Quest.UserProfileId == userProfileId)
                .Where(qo => qo.Quest.Schedule.Unit != PeriodUnitEnum.None)
                .Where(qo => qo.PeriodStart <= to && qo.PeriodEnd >= from)
                .Include(qo => qo.Quest)
                .OrderBy(qo => qo.PeriodStart)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// The catch-up card's data: elapsed periods inside the grace window that a tap would still change.
        /// Completed and skipped periods are excluded in SQL, so an empty result genuinely means "nothing to
        /// ask about" and the client can hide the card without further reasoning.
        /// </summary>
        public async Task<List<QuestOccurrence>> GetCatchUpCandidatesAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            bool includeCompleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.QuestOccurrences
                .Where(qo => qo.Quest.UserProfileId == userProfileId)
                .Where(qo => qo.PeriodStart <= to && qo.PeriodEnd >= from)
                .Where(qo => qo.SkippedAt == null);

            if (!includeCompleted)
                query = query.Where(qo => qo.CompletedAt == null);

            return await query
                .Include(qo => qo.Quest)
                // The ids here are what let the client undo a catch-up tap it made in an earlier session.
                .Include(qo => qo.Completions)
                .OrderBy(qo => qo.PeriodStart)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(qo => qo.QuestId == questId)
                .OrderBy(qo => qo.PeriodStart)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
