using Application.Interfaces.Quests;
using Application.Models;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestRewardCalculator : IQuestRewardCalculator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuestRewardCalculator> _logger;

        public QuestRewardCalculator(IUnitOfWork unitOfWork, ILogger<QuestRewardCalculator> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<QuestRewards> CalculateRewardsAsync(Quest quest, Instant completionTime, CancellationToken cancellationToken = default)
        {
            var rewards = new QuestRewards();

            var userGoal = await _unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(quest.Id, cancellationToken).ConfigureAwait(false);

            if (userGoal is not null)
            {
                userGoal.IsAchieved = true;
                userGoal.AchievedAt = completionTime.ToDateTimeUtc();

                rewards.GoalAchieved = true;
                rewards.UserGoal = userGoal;
                rewards.TotalXp += userGoal.XpBonus;

                _logger.LogInformation("User achieved goal ID {GoalId} and earned bonus {XpBonus} XP", userGoal.Id, userGoal.XpBonus);
            }

            return rewards;
        }
    }
}
