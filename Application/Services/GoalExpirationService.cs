using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class GoalExpirationService : IGoalExpirationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GoalExpirationService> _logger;

        public GoalExpirationService(IUnitOfWork unitOfWork, ILogger<GoalExpirationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> ExpireGoalsAndSaveAsync(CancellationToken cancellationToken = default)
        {
            var expiredGoals = await _unitOfWork.UserGoals.GetGoalsToExpireAsync(cancellationToken).ConfigureAwait(false);

            if (!expiredGoals.Any())
            {
                _logger.LogInformation("No goals to expire at this time.");
                return 0;
            }

            foreach (var goal in expiredGoals)
            {
                goal.IsExpired = true;
            }

            var expiredGoalByAccount = expiredGoals
                .GroupBy(g => g.AccountId)
                .ToDictionary(g => g.Key, g => g.Count());

            var accountIds = expiredGoalByAccount.Keys.ToHashSet();
            var userProfiles = await _unitOfWork.UserProfiles.GetProfilesByAccountIdsAsync(accountIds, cancellationToken).ConfigureAwait(false);

            foreach (var profile in userProfiles)
            {
                if (expiredGoalByAccount.TryGetValue(profile.AccountId, out var count))
                {
                    profile.ExpiredGoals += count;
                    profile.ActiveGoals = Math.Max(0, profile.ActiveGoals - count);
                }
            }

            _logger.LogInformation("Expiring {GoalCount} goals for {ProfileCount} user profiles.", expiredGoals.Count(), userProfiles.Count());

            return await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
