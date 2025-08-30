using Application.Common.Interfaces;
using MediatR;

namespace Application.UserGoals.Commands.CreateUserGoal
{
    public record CreateUserGoalCommand(string GoalType, int QuestId, int UserProfileId) : ICommand<Unit>, ICurrentUserQuestCommand;
}
