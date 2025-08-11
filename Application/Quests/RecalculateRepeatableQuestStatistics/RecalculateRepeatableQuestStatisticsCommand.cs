using MediatR;

namespace Application.Quests.RecalculateRepeatableQuestStatistics
{
    public record RecalculateRepeatableQuestStatisticsCommand() : IRequest<int>;
}
