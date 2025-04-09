using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class QuestLabelRepository : IQuestLabelRepository
    {
        private readonly AppDbContext _context;

        public QuestLabelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuestLabel?> GetLabelByIdAsync(int labelId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .FirstOrDefaultAsync(ql => ql.Id == labelId, cancellationToken)
                .ConfigureAwait(false) ?? null;
        }

        public async Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .Where(ql => ql.AccountId == accountId)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CreateLabelAsync(QuestLabel label, CancellationToken cancellationToken = default)
        {
            await _context.QuestLabels.AddAsync(label, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateLabelAsync(QuestLabel label, CancellationToken cancellationToken = default)
        {
            _context.QuestLabels.Update(label);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task DeleteLabelAsync(QuestLabel label, CancellationToken cancellationToken = default)
        {
            _context.QuestLabels.Remove(label);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task DeleteQuestLabelsByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var questLabelsToDelete = await _context.QuestLabels
                .Where(ql => ql.AccountId == accountId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (questLabelsToDelete.Count != 0)
            {
                _context.QuestLabels.RemoveRange(questLabelsToDelete);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
