using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class QuestLabelRepository : BaseRepository<QuestLabel>, IQuestLabelRepository
    {

        public QuestLabelRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int accountId, bool asNoTracking = false, CancellationToken cancellationToken = default)
        {
            var query = _context.QuestLabels.Where(ql => ql.AccountId == accountId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuestLabel?> GetLabelByValueAsync(string value, int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .FirstOrDefaultAsync(ql => ql.Value == value && ql.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false) ?? null;
        }

        public async Task<bool> IsLabelOwnedByUserAsync(int labelId, int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .AnyAsync(ql => ql.Id == labelId && ql.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> CountOwnedLabelsAsync(IEnumerable<int> labelIds, int accountId, CancellationToken cancellationToken = default)
        {
            if (labelIds == null || !labelIds.Any())
            {
                return 0; // No labels to count
            }

            return await _context.QuestLabels
                .AsNoTracking()
                .CountAsync(ql => labelIds.Contains(ql.Id) && ql.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsLabelValueUniqueForUser(string value, int accountId, CancellationToken cancellationToken = default)
        {
            return !await _context.QuestLabels
                .AnyAsync(ql => ql.Value == value && ql.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}