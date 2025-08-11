using MediatR;

namespace Application.Quests.Commands.ResetCompletedQuests
{
    public record ResetCompletedQuestsCommand() : IRequest<int>;
}
