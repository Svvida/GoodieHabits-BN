using Domain.Enum;
using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestRepository : BaseRepository<Quest>, IQuestRepository
    {
        public QuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(
            int accountId,
            DateTime todayStart,
            DateTime todayEnd,
            SeasonEnum currentSeason,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _context.Quests
                .Where(q => q.AccountId == accountId)
                .Where(q =>
                    (q.QuestType == QuestTypeEnum.OneTime &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        (q.StartDate.HasValue || q.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Daily &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart)

                    || (q.QuestType == QuestTypeEnum.Weekly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        q.WeeklyQuest_Days.Any(wd =>
                            wd.Weekday == (WeekdayEnum)todayStart.DayOfWeek ||
                            wd.Weekday == (WeekdayEnum)todayEnd.DayOfWeek))

                    || (q.QuestType == QuestTypeEnum.Monthly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        ((q.MonthlyQuest_Days!.StartDay <= todayStart.Day && q.MonthlyQuest_Days.EndDay >= todayStart.Day)
                        ||
                        (q.MonthlyQuest_Days.StartDay <= todayEnd.Day && q.MonthlyQuest_Days.EndDay >= todayEnd.Day)))

                    || (q.QuestType == QuestTypeEnum.Seasonal &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        (q.SeasonalQuest_Season!.Season == currentSeason))
                )
                .Include(q => q.WeeklyQuest_Days)
                .Include(q => q.MonthlyQuest_Days)
                .Include(q => q.SeasonalQuest_Season)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel)
                .AsNoTracking();

            return await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetQuestsByTypeForDisplayAsync(
            int accountId,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Quests
                .Where(q => q.AccountId == accountId && q.QuestType == questType)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel)
                .AsNoTracking();

            if (questType == QuestTypeEnum.Daily)
                query = query.Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Monthly)
                query = query.Include(q => q.MonthlyQuest_Days).Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Weekly)
                query = query.Include(q => q.WeeklyQuest_Days).Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Seasonal)
                query = query.Include(q => q.SeasonalQuest_Season);

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Quests
                .Where(q => q.Id == questId && q.QuestType == questType);

            query = query
                .Include(q => q.Account)
                     .ThenInclude(a => a.Profile)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel);

            if (questType == QuestTypeEnum.Daily)
                query = query.Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Monthly)
                query = query.Include(q => q.MonthlyQuest_Days).Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Weekly)
                query = query.Include(q => q.WeeklyQuest_Days).Include(q => q.Statistics);

            else if (questType == QuestTypeEnum.Seasonal)
                query = query.Include(q => q.SeasonalQuest_Season);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestForDisplayAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var query = _context.Quests
                .Where(q => q.Id == questId && q.QuestType == questType)
                .AsNoTracking()
                .Include(q => q.Statistics)
                .Include(q => q.Quest_QuestLabels)
                    .ThenInclude(ql => ql.QuestLabel)
                .AsQueryable();

            if (questType == QuestTypeEnum.Monthly)
            {
                query = query.Include(q => q.MonthlyQuest_Days);
            }
            if (questType == QuestTypeEnum.Weekly)
            {
                query = query.Include(q => q.WeeklyQuest_Days);
            }
            if (questType == QuestTypeEnum.Seasonal)
            {
                query = query.Include(q => q.SeasonalQuest_Season);
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetRepeatableQuestsAsync(bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Quests
                .Include(q => q.Account)
                .Include(q => q.QuestOccurrences)
                .Include(q => q.WeeklyQuest_Days)
                .Include(q => q.MonthlyQuest_Days)
                    .Where(q => q.QuestType == QuestTypeEnum.Daily ||
                                q.QuestType == QuestTypeEnum.Weekly ||
                                q.QuestType == QuestTypeEnum.Monthly);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetRepeatableQuestsForStatsProcessingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Where(q => q.QuestType == QuestTypeEnum.Daily ||
                            q.QuestType == QuestTypeEnum.Weekly ||
                            q.QuestType == QuestTypeEnum.Monthly)
                .Include(q => q.Statistics)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<IEnumerable<Quest>> GetRepeatableQuestsForOccurrencesProcessingAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            var query = _context.Quests
                .Where(q => q.QuestType == QuestTypeEnum.Daily ||
                            q.QuestType == QuestTypeEnum.Weekly ||
                            q.QuestType == QuestTypeEnum.Monthly)
                .Where(q => (q.EndDate ?? DateTime.MaxValue) >= nowUtc &&
                            (q.StartDate ?? DateTime.MinValue) <= nowUtc)
                .Include(q => q.QuestOccurrences)
                .Include(q => q.Account)
                .Include(q => q.WeeklyQuest_Days)
                .Include(q => q.MonthlyQuest_Days)
                    .AsNoTracking();

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsQuestOwnedByUserAsync(
            int questId,
            int accountId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .AnyAsync(q => q.Id == questId &&
                        q.AccountId == accountId,
                        cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestForStatsProcessingAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Include(q => q.Statistics)
                .FirstOrDefaultAsync(q => q.Id == questId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int accountId, DateTime now, CancellationToken cancellationToken = default)
        {
            var activeUserGoalsIds = await _context.UserGoals
                .Where(g => g.AccountId == accountId && !g.IsExpired)
                .AsNoTracking()
                .Select(g => g.QuestId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return await _context.Quests
                .Where(q => q.AccountId == accountId &&
                            !q.IsCompleted &&
                            (q.EndDate ?? DateTime.MaxValue) > now &&
                            !activeUserGoalsIds.Contains(q.Id))
                .Include(q => q.WeeklyQuest_Days)
                .Include(q => q.MonthlyQuest_Days)
                .Include(q => q.SeasonalQuest_Season)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<IEnumerable<Quest>> GetQuestsEligibleForResetAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Where(q => q.IsCompleted &&
                       (q.EndDate ?? DateTime.MaxValue) >= DateTime.UtcNow &&
                       (q.NextResetAt.HasValue && q.NextResetAt <= DateTime.UtcNow))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
