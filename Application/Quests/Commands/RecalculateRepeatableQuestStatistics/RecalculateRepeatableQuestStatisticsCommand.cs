using Application.Common.Interfaces;

namespace Application.Quests.Commands.RecalculateRepeatableQuestStatistics
{
    public record RecalculateRepeatableQuestStatisticsCommand() : ICommand<int>;
}
