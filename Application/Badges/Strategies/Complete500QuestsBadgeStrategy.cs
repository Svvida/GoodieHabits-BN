using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class Complete500QuestsBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.Complete500Quests;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;

            if (userProfile.CompletedQuests < 500)
                return;

            var badge = allBadges.First(b => b.Type == badgeType);
            userProfile.AwardBadge(badge, DateTime.UtcNow);
        }
    }
}
