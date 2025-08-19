using Application.Common.Interfaces;

namespace Application.UserGoals.Commands.ExpireGoals
{
    public record ExpireGoalsCommand() : ICommand<int>;
}
