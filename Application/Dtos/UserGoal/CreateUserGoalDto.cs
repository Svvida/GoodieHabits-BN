using System.Text.Json.Serialization;

namespace Application.Dtos.UserGoal
{
    public class CreateUserGoalDto
    {
        [JsonIgnore]
        public int QuestId { get; set; }
        public string GoalType { get; set; } = null!;
        public string QuestType { get; set; } = null!;
    }
}
