using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class CreateTwentyQuestsBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCreated;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.CreateTwentyQuests;

            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;

            if (userProfile.TotalQuests >= 20)
            {
                var badge = allBadges.First(b => b.Type == badgeType);
                userProfile.AwardBadge(badge, DateTime.UtcNow);
            }
        }
    }
}
