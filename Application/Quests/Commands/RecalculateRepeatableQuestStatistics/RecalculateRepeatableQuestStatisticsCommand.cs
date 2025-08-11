using MediatR;

namespace Application.Quests.Commands.RecalculateRepeatableQuestStatistics
{
    public record RecalculateRepeatableQuestStatisticsCommand() : IRequest<int>;
}
