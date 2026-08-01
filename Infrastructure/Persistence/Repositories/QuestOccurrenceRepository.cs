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
                .Where(qo => qo.Quest.QuestType == QuestTypeEnum.Daily ||
                             qo.Quest.QuestType == QuestTypeEnum.Weekly ||
                             qo.Quest.QuestType == QuestTypeEnum.Monthly)
                .Where(qo => qo.PeriodStart <= to && qo.PeriodEnd >= from)
                .Include(qo => qo.Quest)
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
