using Domain.Enums;
using Domain.Models;

namespace Application.Badges
{
    public interface IBadgeAwardingService
    {
        Task CheckAndAwardBadgesAsync(BadgeTriggerEnum badgeTrigger, UserProfile userProfile, Quest? quest, CancellationToken cancellationToken = default);
    }
}
