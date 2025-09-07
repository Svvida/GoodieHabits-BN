using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class GoalCompleteFiftyBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.GoalCompleteFifty;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;
            if (userProfile.CompletedGoals < 50)
                return;
            var badge = allBadges.First(b => b.Type == badgeType);
            userProfile.AwardBadge(badge, DateTime.UtcNow);
        }
    }
}
