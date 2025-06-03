using Application.Models;
using Domain.Models;
using NodaTime;

namespace Application.Interfaces.Quests
{
    public interface IQuestRewardCalculator
    {
        Task<QuestRewards> CalculateRewardsAsync(Quest quest, Instant completionTime, CancellationToken cancellationToken);
    }
}
