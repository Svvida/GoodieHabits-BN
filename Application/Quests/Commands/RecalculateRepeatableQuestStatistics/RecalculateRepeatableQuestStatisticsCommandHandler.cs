using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.RecalculateRepeatableQuestStatistics
{
    public class RecalculateRepeatableQuestStatisticsCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RecalculateRepeatableQuestStatisticsCommand, int>
    {
        public async Task<int> Handle(RecalculateRepeatableQuestStatisticsCommand request, CancellationToken cancellationToken)
        {
            var utcNow = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var quests = await unitOfWork.Quests.GetRepeatableQuestsForStatsProcessingAsync(utcNow, cancellationToken).ConfigureAwait(false);

            if (!quests.Any())
                return 0; // No quests to process

            foreach (var quest in quests)
            {
                quest.RecalculateStatistics(utcNow);
            }

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}