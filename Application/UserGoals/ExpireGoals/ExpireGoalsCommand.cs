using MediatR;

namespace Application.UserGoals.ExpireGoals
{
    public record ExpireGoalsCommand() : IRequest<int>;
}
