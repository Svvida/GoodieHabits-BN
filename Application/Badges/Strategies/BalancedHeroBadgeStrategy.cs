using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class BalancedHeroBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.BalancedHero;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;

            if (userProfile.CompletedDailyQuests >= 10 && userProfile.CompletedWeeklyQuests >= 10 &&
               userProfile.CompletedMonthlyQuests >= 10)
            {
                var badge = allBadges.First(b => b.Type == badgeType);
                userProfile.AwardBadge(badge, DateTime.UtcNow);
                return;
            }
        }
    }
}
