using Domain.Interfaces.Repositories;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IAccountRepository Accounts { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserGoalRepository UserGoals { get; }
        IQuestRepository Quests { get; }
        IQuestOccurrenceRepository QuestOccurrences { get; }
        IQuestLabelRepository QuestLabels { get; }
        INotificationRepository Notifications { get; }
        IBadgeRepository Badges { get; }
        IFriendsRepository Friends { get; }
        IUserBlockRepository UserBlocks { get; }
        IFriendInvitationRepository FriendInvitations { get; }
        IShopItemRepository ShopItems { get; }
        IUserInventoryRepository UserInventories { get; }
        IFinanceCategoryRepository FinanceCategories { get; }
        IFinanceTransactionRepository FinanceTransactions { get; }
        IBudgetRepository Budgets { get; }
        IRecurringTransactionRepository RecurringTransactions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
