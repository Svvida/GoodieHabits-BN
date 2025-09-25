using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IBadgeRepository : IBaseRepository<Badge>
    {
        Task<Badge> GetBadgeByTypeAsync(BadgeTypeEnum badgeType, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken = default);
    }
}
