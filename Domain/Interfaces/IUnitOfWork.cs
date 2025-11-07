using Domain.Interfaces.Repositories;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IAccountRepository Accounts { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserGoalRepository UserGoals { get; }
        IQuestRepository Quests { get; }
        IQuestLabelRepository QuestLabels { get; }
        INotificationRepository Notifications { get; }
        IBadgeRepository Badges { get; }
        IFriendsRepository Friends { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
