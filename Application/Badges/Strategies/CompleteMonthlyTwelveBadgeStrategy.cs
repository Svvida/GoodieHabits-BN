using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class CompleteMonthlyTwelveBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.CompleteMonthlyTwelve;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;
            if (quest is null || quest.QuestType != QuestTypeEnum.Monthly)
                return;
            if (quest.Statistics is not null && quest.Statistics.LongestStreak >= 12)
            {
                var badge = allBadges.First(b => b.Type == badgeType);
                userProfile.AwardBadge(badge, DateTime.UtcNow);
            }
        }
    }
}
