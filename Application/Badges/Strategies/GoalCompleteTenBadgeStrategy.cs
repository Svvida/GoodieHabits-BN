using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class GoalCompleteTenBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? goal, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.GoalCompleteTen;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;
            if (userProfile.CompletedGoals < 10)
                return;
            var badge = allBadges.First(b => b.Type == badgeType);
            userProfile.AwardBadge(badge, DateTime.UtcNow);
        }
    }
}
