using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Quests
{
    public class QuestOccurrenceRepository(AppDbContext context) : BaseRepository<QuestOccurrence>(context), IQuestOccurrenceRepository
    {
        public async Task<bool> IsQuestOccurrenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .AnyAsync(q =>
                    q.QuestId == questId &&
                    q.OccurrenceStart == occurenceStart &&
                    q.OccurrenceEnd == occurenceEnd,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QuestOccurrence?> GetCurrentOccurrenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .FirstOrDefaultAsync(q =>
                    q.QuestId == questId &&
                    q.OccurrenceStart <= now &&
                    q.OccurrenceEnd >= now,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QuestOccurrence?> GetLastOccurrenceForCompletionAsync(int questId, DateTime now, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .OrderByDescending(q => q.OccurrenceEnd)
                .FirstOrDefaultAsync(q =>
                    q.QuestId == questId &&
                    q.OccurrenceEnd <= now,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(q => q.QuestId == questId)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
