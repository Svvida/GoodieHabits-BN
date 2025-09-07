using Domain.Enums;
using Domain.Models;

namespace Application.Common.Interfaces.Badges
{
    public interface IBadgeAwardingStrategy
    {
        BadgeTriggerEnum Trigger { get; }
        void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges);
    }
}
