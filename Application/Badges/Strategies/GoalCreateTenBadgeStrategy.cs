using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class GoalCreateTenBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.GoalCreated;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.GoalCreateTen;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;
            if (userProfile.TotalGoals < 10)
                return;
            var badge = allBadges.First(b => b.Type == badgeType);
            userProfile.AwardBadge(badge, DateTime.UtcNow);
        }
    }
}
