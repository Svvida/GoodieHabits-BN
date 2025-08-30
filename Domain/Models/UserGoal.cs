using Domain.Common;
using Domain.Enums;
using Domain.Events.UserGoals;
using Domain.Exceptions;

namespace Domain.Models
{
    public class UserGoal : AggregateRoot
    {
        public int Id { get; set; }
        public int QuestId { get; private set; }
        public int UserProfileId { get; private set; }
        public GoalTypeEnum GoalType { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime EndsAt { get; private set; }
        public bool IsAchieved { get; set; } = false;
        public DateTime? AchievedAt { get; private set; } = null;
        public bool IsExpired { get; set; } = false;
        public int XpBonus { get; private set; } = 5;

        public Quest Quest { get; set; } = null!;
        public UserProfile UserProfile { get; set; } = null!;

        protected UserGoal() { }
        private UserGoal(int questId, int userProfileId, GoalTypeEnum goalType, DateTime endsAt, int xpBonus)
        {
            QuestId = questId;
            UserProfileId = userProfileId;
            GoalType = goalType;
            EndsAt = endsAt;
            XpBonus = xpBonus;

            AddDomainEvent(new UserGoalCreatedEvent(userProfileId));
        }

        public static UserGoal Create(int questId, int userProfileId, GoalTypeEnum goalType, DateTime endsAt, int xpBonus)
        {
            return new UserGoal(questId, userProfileId, goalType, endsAt, xpBonus);
        }

        public void MarkAsAchieved(DateTime achievedAt)
        {
            if (achievedAt < CreatedAt)
                throw new InvalidArgumentException("Achieved date cannot be before the creation date.");
            AchievedAt = achievedAt;
            IsAchieved = true;
        }

        public bool Expire(DateTime nowUtc)
        {
            if (IsExpired)
                return false;

            if (nowUtc < EndsAt)
                return false;

            IsExpired = true;
            return true;
        }
    }
}
