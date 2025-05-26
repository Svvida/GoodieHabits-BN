using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestOccurrenceRepository : IQuestOccurrenceRepository
    {
        private readonly AppDbContext _context;

        public QuestOccurrenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveOccurrencesAsync(List<QuestOccurrence> occurences, CancellationToken cancellationToken = default)
        {
            _context.QuestOccurrences.AddRange(occurences);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateOccurrence(QuestOccurrence occurence, CancellationToken cancellationToken = default)
        {
            _context.QuestOccurrences.Update(occurence);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

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

        public async Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(q => q.QuestId == questId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
