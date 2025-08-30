using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class QuestLabelRepository(AppDbContext context) : BaseRepository<QuestLabel>(context), IQuestLabelRepository
    {
        public async Task<QuestLabel?> GetUserLabelByIdAsync(int labelId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.QuestLabels.Where(ql => ql.Id == labelId && ql.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int userProfileId, bool asNoTracking = false, CancellationToken cancellationToken = default)
        {
            var query = _context.QuestLabels.Where(ql => ql.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsLabelOwnedByUserAsync(int labelId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .AnyAsync(ql => ql.Id == labelId && ql.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> CountOwnedLabelsAsync(IEnumerable<int> labelIds, int userProfileId, CancellationToken cancellationToken = default)
        {
            if (labelIds == null || !labelIds.Any())
                return 0; // No labels to count

            return await _context.QuestLabels
                .AsNoTracking()
                .CountAsync(ql => labelIds.Contains(ql.Id) && ql.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsLabelValueUniqueForUser(string value, int userProfileId, CancellationToken cancellationToken = default)
        {
            return !await _context.QuestLabels
                .AnyAsync(ql => ql.Value == value && ql.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}