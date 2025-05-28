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
        private readonly IUserGoalRepository _userGoalRepository;
        private readonly ILogger<QuestRewardCalculator> _logger;

        public QuestRewardCalculator(IUserGoalRepository userGoalRepository, ILogger<QuestRewardCalculator> logger)
        {
            _userGoalRepository = userGoalRepository;
            _logger = logger;
        }

        public async Task<QuestRewards> CalculateRewardsAsync(Quest quest, Instant completionTime, CancellationToken cancellationToken = default)
        {
            var rewards = new QuestRewards();

            var userGoal = await _userGoalRepository.GetActiveGoalByQuestIdAsync(quest.Id, cancellationToken).ConfigureAwait(false);

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
