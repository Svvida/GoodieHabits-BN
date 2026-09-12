using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class QuestRepository(AppDbContext context) : BaseRepository<Quest>(context), IQuestRepository
    {
        /// <summary>
        /// Quests whose active range covers today. The schedule's own rules (weekdays, intervals, month and
        /// year windows) are applied in memory by the caller rather than mirrored here: a second copy of the
        /// scheduling logic in SQL is exactly what drifted apart before, and a user has tens of quests.
        /// </summary>
        public async Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(
            int userProfileId,
            DateOnly today,
            CancellationToken cancellationToken = default)
        {
            return await WithDisplayIncludes(_context.Quests
                    .Where(q => q.UserProfileId == userProfileId)
                    .Where(q => (q.StartDate ?? DateOnly.MinValue) <= today &&
                                (q.EndDate ?? DateOnly.MaxValue) >= today))
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetQuestsForDisplayAsync(
            int userProfileId,
            PeriodUnitEnum? unit,
            QuestTypeEnum? legacyType,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Quests.Where(q => q.UserProfileId == userProfileId);

            if (unit.HasValue)
                query = query.Where(q => q.Schedule.Unit == unit.Value);

            // The retired quest types, expressed against the schedule that replaced them. Weekly is the only
            // interesting one: it split, so it has to gather both halves back together.
            query = legacyType switch
            {
                QuestTypeEnum.OneTime => query.Where(q => q.Schedule.Unit == PeriodUnitEnum.None),
                QuestTypeEnum.Daily => query.Where(q => q.Schedule.Unit == PeriodUnitEnum.Day && q.Schedule.Weekdays == null),
                QuestTypeEnum.Weekly => query.Where(q =>
                    q.Schedule.Unit == PeriodUnitEnum.Week ||
                    (q.Schedule.Unit == PeriodUnitEnum.Day && q.Schedule.Weekdays != null)),
                QuestTypeEnum.Monthly => query.Where(q => q.Schedule.Unit == PeriodUnitEnum.Month),
                QuestTypeEnum.Seasonal => query.Where(q => q.Schedule.Unit == PeriodUnitEnum.Year),
                _ => query
            };

            return await WithDisplayIncludes(query)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestByIdAsync(int questId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = WithDisplayIncludes(_context.Quests
                .Where(q => q.Id == questId && q.UserProfileId == userProfileId));

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestForCompletionAsync(int questId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Where(q => q.Id == questId && q.UserProfileId == userProfileId)
                // Every period and every completion, not just recent ones: the entity de-duplicates periods
                // in memory against this collection, and resolves idempotency keys against that one, so a
                // partial load would produce a row colliding with a unique index.
                .Include(q => q.QuestOccurrences)
                .Include(q => q.Completions)
                .Include(q => q.Statistics)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel)
                .Include(q => q.UserProfile)
                    .ThenInclude(up => up.UserProfile_Badges)
                        .ThenInclude(upb => upb.Badge)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestByIdForUpdateAsync(int questId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Where(q => q.Id == questId && q.UserProfileId == userProfileId)
                .Include(q => q.QuestOccurrences)
                    .ThenInclude(qo => qo.Completions)
                .Include(q => q.Statistics)
                .Include(q => q.UserProfile)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsQuestOwnedByUserAsync(int questId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .AnyAsync(q => q.Id == questId && q.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int userProfileId, DateOnly today, CancellationToken cancellationToken = default)
        {
            var activeUserGoalsIds = await _context.UserGoals
                .Where(g => g.UserProfileId == userProfileId && !g.IsExpired)
                .AsNoTracking()
                .Select(g => g.QuestId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // "Already completed" is no longer a column. A repeating quest is always eligible while it is
            // active; a one-time one is spent once it has ever been completed.
            return await WithDisplayIncludes(_context.Quests
                    .Where(q => q.UserProfileId == userProfileId &&
                                (q.EndDate ?? DateOnly.MaxValue) >= today &&
                                !activeUserGoalsIds.Contains(q.Id))
                    .Where(q => q.Schedule.Unit != PeriodUnitEnum.None || !q.WasEverCompleted))
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestWithUserProfileAsync(int questId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Include(q => q.UserProfile)
                    .ThenInclude(up => up.UserProfile_Badges)
                        .ThenInclude(upb => upb.Badge)
                .FirstOrDefaultAsync(q => q.Id == questId && q.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Quest?> GetUserQuestByIdAsync(int questId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questId && q.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// What every read path needs: the profile (for the local date the DTO is built against), the
        /// period around today, the statistics cache and the labels. One include list instead of the
        /// per-type branching the old repository carried.
        /// <para>
        /// The occurrence include is <b>filtered to a two-day window</b> around the server's date, which is
        /// wide enough to contain the user's current period in any timezone while keeping a five-year-old
        /// habit's history out of a list query. This is safe only because these paths are read-only:
        /// anything that generates periods must load every one of them, or its in-memory de-duplication
        /// would miss and collide with the unique index.
        /// </para>
        /// </summary>
        private static IQueryable<Quest> WithDisplayIncludes(IQueryable<Quest> query)
        {
            var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
            var earliest = utcToday.AddDays(-1);
            var latest = utcToday.AddDays(1);

            return query
                .Include(q => q.UserProfile)
                .Include(q => q.Statistics)
                .Include(q => q.QuestOccurrences.Where(o => o.PeriodEnd >= earliest && o.PeriodStart <= latest))
                    // The current period's taps: their ids are what makes undo work after a restart, and
                    // their dates are what makes the per-day cap enforceable in the UI.
                    .ThenInclude(o => o.Completions)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel);
        }
    }
}
