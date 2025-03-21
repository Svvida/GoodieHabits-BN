﻿using Domain.Interfaces;
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

        public async Task<QuestLabel?> GetLabelByIdAsync(int labelId, int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .FirstOrDefaultAsync(ql => ql.Id == labelId && ql.AccountId == accountId, cancellationToken)
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
            var joinRecords = await _context.QuestMetadata_QuestLabels
                .Where(qml => qml.QuestLabelId == label.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _context.QuestMetadata_QuestLabels.RemoveRange(joinRecords);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _context.QuestLabels.Remove(label);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuestLabel?> GetLabelByValueAsync(string value, int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestLabels
                .FirstOrDefaultAsync(ql => ql.Value == value && ql.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false) ?? null;
        }
    }
}
