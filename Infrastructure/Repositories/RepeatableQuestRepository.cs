using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RepeatableQuestRepository : IRepeatableQuestRepository
    {
        private readonly AppDbContext _context;

        public RepeatableQuestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RepeatableQuest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.RepeatableQuests
                .Include(rq => rq.Quest)
                .AsNoTracking()
                .FirstOrDefaultAsync(rq => rq.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<RepeatableQuest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.RepeatableQuests
                .Include(rq => rq.Quest)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task AddAsync(RepeatableQuest repeatableQuest, CancellationToken cancellationToken = default)
        {
            var quest = new Quest
            {
                QuestType = Domain.Enum.QuestType.Repeatable,
                AccountId = repeatableQuest.Quest.AccountId,
            };

            repeatableQuest.Id = quest.Id;

            await _context.RepeatableQuests.AddAsync(repeatableQuest, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(RepeatableQuest repeatableQuest, CancellationToken cancellationToken = default)
        {
            _context.RepeatableQuests.Attach(repeatableQuest);

            _context.Entry(repeatableQuest).State = EntityState.Modified;

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _context.Quests.FirstOrDefaultAsync(q => q.Id == id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {id} not found.");

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<RepeatableQuest>> GetByTypesAsync(List<string> types, CancellationToken cancellationToken = default)
        {
            if (types is null || types.Count == 0)
                throw new InvalidArgumentException("Types list cannot be empty");

            return await _context.RepeatableQuests
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ContinueWith(task => task.Result
                    .AsEnumerable()
                    .Where(rq => types.Contains(rq.RepeatInterval.Type)), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
