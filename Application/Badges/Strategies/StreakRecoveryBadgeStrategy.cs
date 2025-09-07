using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class StreakRecoveryBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.StreakRecovery;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;

            if (quest is null || !quest.IsRepeatable())
                return;

            if (quest.Statistics is not null && quest.Statistics.CurrentStreak >= 1 && quest.Statistics.LongestStreak >= 7)
            {
                var badge = allBadges.First(b => b.Type == badgeType);
                userProfile.AwardBadge(badge, DateTime.UtcNow);
            }
        }
    }
}
