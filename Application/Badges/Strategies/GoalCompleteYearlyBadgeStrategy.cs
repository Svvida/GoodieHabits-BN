using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Models;

namespace Application.Badges.Strategies
{
    public class GoalCompleteYearlyBadgeStrategy : IBadgeAwardingStrategy
    {
        public BadgeTriggerEnum Trigger => BadgeTriggerEnum.QuestCompleted;
        public void Apply(UserProfile userProfile, Quest? quest, IReadOnlyCollection<Badge> allBadges)
        {
            var badgeType = BadgeTypeEnum.GoalCompleteYearly;
            if (userProfile.UserProfile_Badges.Any(upb => upb.Badge.Type == badgeType))
                return;
            if (quest is null || quest.UserGoal.Count <= 0)
                return;

            foreach (var goal in quest.UserGoal)
            {
                if (goal.GoalType == GoalTypeEnum.Yearly && quest.IsCompleted)
                {
                    var badge = allBadges.First(b => b.Type == badgeType);
                    userProfile.AwardBadge(badge, DateTime.UtcNow);
                    return;
                }
            }
        }
    }
}
