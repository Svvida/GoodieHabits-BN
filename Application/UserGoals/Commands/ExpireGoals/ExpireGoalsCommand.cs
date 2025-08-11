using MediatR;

namespace Application.UserGoals.Commands.ExpireGoals
{
    public record ExpireGoalsCommand() : IRequest<int>;
}
