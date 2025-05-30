using Domain.Models;

namespace Application.Models
{
    public class QuestRewards
    {
        public int TotalXp { get; set; } = 10; // Default XP reward
        public bool GoalAchieved { get; set; }
        public UserGoal? UserGoal { get; set; }
    }
}
