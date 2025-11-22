using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        private IAccountRepository? _accountRepository;
        private IUserProfileRepository? _userProfileRepository;
        private IUserGoalRepository? _userGoalRepository;
        private IQuestRepository? _questRepository;
        private IQuestLabelRepository? _questLabelRepository;
        private INotificationRepository? _notificationRepository;
        private IBadgeRepository? _badgeRepository;
        private IFriendsRepository? _friendsRepository;
        private IUserBlockRepository? _userBlockRepository;
        private IFriendInvitationRepository? _friendInvitationRepository;
        private IShopItemRepository? _shopItemRepository;
        private IUserInventoryRepository? _userInventoryRepository;

        public IAccountRepository Accounts => _accountRepository ??= new AccountRepository(_context);
        public IUserProfileRepository UserProfiles => _userProfileRepository ??= new UserProfileRepository(_context);
        public IUserGoalRepository UserGoals => _userGoalRepository ??= new UserGoalRepository(_context);
        public IQuestRepository Quests => _questRepository ??= new QuestRepository(_context);
        public IQuestLabelRepository QuestLabels => _questLabelRepository ??= new QuestLabelRepository(_context);
        public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);
        public IBadgeRepository Badges => _badgeRepository ??= new BadgeRepository(_context);
        public IFriendsRepository Friends => _friendsRepository ??= new FriendsRepository(_context);
        public IUserBlockRepository UserBlocks => _userBlockRepository ??= new UserBlockRepository(_context);
        public IFriendInvitationRepository FriendInvitations => _friendInvitationRepository ??= new FriendInvitationRepository(_context);
        public IShopItemRepository ShopItems => _shopItemRepository ??= new ShopItemRepository(_context);
        public IUserInventoryRepository UserInventories => _userInventoryRepository ??= new UserInventoryRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}