using MediatR;

namespace Application.Quests.ResetCompletedQuests
{
    public record ResetCompletedQuestsCommand() : IRequest<int>;
}
