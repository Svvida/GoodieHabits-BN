using System.Text.Json.Serialization;
using MediatR;

namespace Application.UserGoals.Commands.CreateUserGoal
{
    public record CreateUserGoalCommand(string GoalType, string QuestType) : IRequest<Unit>
    {
        [JsonIgnore]
        public int QuestId { get; set; }
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
