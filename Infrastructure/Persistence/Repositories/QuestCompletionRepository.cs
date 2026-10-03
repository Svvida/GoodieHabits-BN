using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class QuestCompletionRepository(AppDbContext context)
        : BaseRepository<QuestCompletion>(context), IQuestCompletionRepository
    {
        public async Task<List<QuestCompletion>> GetForQuestInRangeAsync(
            int questId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return await _context.QuestCompletions
                .Where(qc => qc.QuestId == questId)
                .Where(qc => qc.CompletedOn >= from && qc.CompletedOn <= to)
                .OrderBy(qc => qc.CompletedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<QuestCompletion>> GetForUserInRangeAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return await _context.QuestCompletions
                .Where(qc => qc.UserProfileId == userProfileId)
                .Where(qc => qc.CompletedOn >= from && qc.CompletedOn <= to)
                .OrderBy(qc => qc.CompletedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
