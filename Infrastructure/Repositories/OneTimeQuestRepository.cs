using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class OneTimeQuestRepository : IOneTimeQuestRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OneTimeQuestRepository> _logger;

        public OneTimeQuestRepository(
            AppDbContext context,
            ILogger<OneTimeQuestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OneTimeQuest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OneTimeQuests
                .Include(otq => otq.Quest)
                .AsNoTracking()
                .FirstOrDefaultAsync(otq => otq.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<OneTimeQuest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.OneTimeQuests
                .Include(otq => otq.Quest)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task AddAsync(OneTimeQuest oneTimeQuest, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("We are in repository");
            var quest = new Quest
            {
                QuestType = QuestType.OneTime,
                AccountId = oneTimeQuest.Quest.AccountId,
            };

            oneTimeQuest.Id = quest.Id;

            await _context.OneTimeQuests.AddAsync(oneTimeQuest, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(OneTimeQuest oneTimeQuest, CancellationToken cancellationToken = default)
        {
            _context.OneTimeQuests.Attach(oneTimeQuest);

            _context.Entry(oneTimeQuest).State = EntityState.Modified;

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _context.Quests.FirstOrDefaultAsync(q => q.Id == id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {id} not found.");

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
