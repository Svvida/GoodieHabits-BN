using Domain.Enum;

namespace Domain.Models
{
    public class UserGoal
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public int AccountId { get; set; }
        public GoalTypeEnum GoalType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EndsAt { get; set; }
        public bool IsAchieved { get; set; } = false;
        public DateTime? AchievedAt { get; set; } = null;
        public bool IsExpired { get; set; } = false;
        public int XpBonus { get; set; } = 5;

        public Quest Quest { get; set; } = null!;
        public Account Account { get; set; } = null!;

        public UserGoal() { }
        public UserGoal(int questId, int accountId, GoalTypeEnum goalType, DateTime endsAt)
        {
            QuestId = questId;
            AccountId = accountId;
            GoalType = goalType;
            EndsAt = endsAt;
        }
    }
}
