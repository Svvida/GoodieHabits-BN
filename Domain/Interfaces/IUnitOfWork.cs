using Domain.Interfaces.Quests;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IAccountRepository Accounts { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserGoalRepository UserGoals { get; }
        IQuestRepository Quests { get; }
        IQuestLabelRepository QuestLabels { get; }
        IQuestOccurrenceRepository QuestOccurrences { get; }
        IQuestStatisticsRepository QuestStatistics { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
