using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestMetadataRepository : IQuestMetadataRepository
    {
        private readonly AppDbContext _context;

        public QuestMetadataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuestMetadata>> GetTodaysQuestsAsync(CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.UtcNow.Date;

            return await _context.QuestsMetadata
                .Include(q => q.OneTimeQuest)
                .Include(q => q.DailyQuest)
                .Include(q => q.WeeklyQuest)
                .Include(q => q.MonthlyQuest)
                .Include(q => q.SeasonalQuest)
                .Where(q =>
                    (q.OneTimeQuest != null &&
                    (!q.OneTimeQuest.StartDate.HasValue || q.OneTimeQuest.StartDate.Value.Date <= today) &&
                    (!q.OneTimeQuest.EndDate.HasValue || q.OneTimeQuest.EndDate.Value.Date >= today)) ||

                    (q.DailyQuest != null &&
                    (!q.DailyQuest.StartDate.HasValue || q.DailyQuest.StartDate.Value.Date <= today) &&
                    (!q.DailyQuest.EndDate.HasValue || q.DailyQuest.EndDate.Value.Date >= today)) ||

                    (q.WeeklyQuest != null &&
                    (!q.WeeklyQuest.StartDate.HasValue || q.WeeklyQuest.StartDate.Value.Date <= today) &&
                    (!q.WeeklyQuest.EndDate.HasValue || q.WeeklyQuest.EndDate.Value.Date >= today)) ||

                    (q.MonthlyQuest != null &&
                    (!q.MonthlyQuest.StartDate.HasValue || q.MonthlyQuest.StartDate.Value.Date <= today) &&
                    (!q.MonthlyQuest.EndDate.HasValue || q.MonthlyQuest.EndDate.Value.Date >= today)) ||

                    (q.SeasonalQuest != null &&
                    (!q.SeasonalQuest.StartDate.HasValue || q.SeasonalQuest.StartDate.Value.Date <= today) &&
                    (!q.SeasonalQuest.EndDate.HasValue || q.SeasonalQuest.EndDate.Value.Date >= today))
                    )
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
