using Application.Common.Interfaces;

namespace Application.Quests.Commands.ResetCompletedQuests
{
    public record ResetCompletedQuestsCommand() : ICommand<int>;
}
