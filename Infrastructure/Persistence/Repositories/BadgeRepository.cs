using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BadgeRepository(AppDbContext context) : BaseRepository<Badge>(context), IBadgeRepository
    {
        public async Task<Badge> GetBadgeByTypeAsync(BadgeTypeEnum badgeType, CancellationToken cancellationToken = default)
        {
            return await _context.Badges.FirstAsync(b => b.Type == badgeType, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Badges.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
