using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestOccurenceRepository : IQuestOccurenceRepository
    {
        private readonly AppDbContext _context;

        public QuestOccurenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveOccurencesAsync(List<QuestOccurrence> occurences, CancellationToken cancellationToken = default)
        {
            _context.QuestOccurrences.AddRange(occurences);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateOccurence(QuestOccurrence occurence, CancellationToken cancellationToken = default)
        {
            _context.QuestOccurrences.Update(occurence);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsQuestOccurenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .AnyAsync(q =>
                    q.QuestId == questId &&
                    q.OccurenceStart == occurenceStart &&
                    q.OccurenceEnd == occurenceEnd,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QuestOccurrence?> GetCurrentOccurenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .FirstOrDefaultAsync(q =>
                    q.QuestId == questId &&
                    q.OccurenceStart <= now &&
                    q.OccurenceEnd >= now,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
