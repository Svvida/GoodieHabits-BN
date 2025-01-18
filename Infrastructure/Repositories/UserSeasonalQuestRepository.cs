using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserSeasonalQuestRepository : BaseRepository<UserSeasonalQuest>, IUserSeasonalQuestRepository
    {
        public UserSeasonalQuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserSeasonalQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSeasonalQuest>()
                .Where(userQuest => userQuest.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }
    }
}
