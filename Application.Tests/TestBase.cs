using Application.Common.Interfaces;
using Application.Common.Interfaces.Email;
using Application.Common.Interfaces.Notifications;
using Application.Statistics.Calculators;
using Domain.Interfaces;
using Domain.Models;
using Domain.ValueObjects;
using Infrastructure.Notifications;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;

namespace Application.Tests
{
    public abstract class TestBase<THandler> : IDisposable
    {
        protected readonly AppDbContext _context;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly Mock<IMediator> _mediatorMock;
        protected readonly Mock<ILogger<THandler>> _loggerMock;
        protected readonly Mock<IClock> _clockMock;
        protected readonly Mock<IForgotPasswordEmailSender> _emailSenderMock;
        protected readonly Instant _fixedTestInstant;
        protected readonly IMapper _mapper;
        protected readonly Mock<IUrlBuilder> _urlBuilderMock;
        protected readonly Mock<ILevelCalculator> _levelCalculatorMock;
        protected readonly Mock<INotificationSender> _notificationSenderMock;
        protected readonly INotificationService _notificationService;

        protected TestBase()
        {
            _fixedTestInstant = Instant.FromUtc(2023, 10, 26, 10, 0, 0);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _unitOfWork = new UnitOfWork(_context);

            _urlBuilderMock = new Mock<IUrlBuilder>();
            // (Optional but good practice) Set up a default behavior for the mock
            _urlBuilderMock.Setup(b => b.BuildThumbnailAvatarUrl(It.IsAny<string>()))
                           .Returns((string publicId) => string.IsNullOrEmpty(publicId) ? string.Empty : $"mock_url_for_{publicId}");
            _urlBuilderMock.Setup(b => b.BuildProfilePageAvatarUrl(It.IsAny<string>()))
                           .Returns((string publicId) => string.IsNullOrEmpty(publicId) ? string.Empty : $"mock_url_for_{publicId}");

            _levelCalculatorMock = new Mock<ILevelCalculator>();
            _levelCalculatorMock
                            .Setup(lc => lc.CalculateLevelInfo(It.IsAny<int>()))
                            .Returns((int xp) => new LevelInfo // Use a callback to return dynamic info
                            {
                                CurrentLevel = xp / 100, // Example: 1 level per 100 XP
                                IsMaxLevel = false,
                                CurrentLevelRequiredXp = (xp / 100) * 100,
                                NextLevelRequiredXp = ((xp / 100) + 1) * 100
                            });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUrlBuilder)))
                    .Returns(_urlBuilderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILevelCalculator)))
                    .Returns(_levelCalculatorMock.Object);

            var typeAdapterConfig = new TypeAdapterConfig();
            typeAdapterConfig.Scan(typeof(Application.AssemblyReference).Assembly);

            _mapper = new ServiceMapper(serviceProviderMock.Object, typeAdapterConfig);

            _notificationSenderMock = new Mock<INotificationSender>();

            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<THandler>>();
            _clockMock = new Mock<IClock>();
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant);
            _emailSenderMock = new Mock<IForgotPasswordEmailSender>();

            _notificationService = new NotificationService(
                _unitOfWork,
                _notificationSenderMock.Object,
                _clockMock.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        protected async Task<Account> AddAccountAsync(string email, string passwordHash, string nickname, string timeZone = "Etc/UTC")
        {
            var account = Account.Create(passwordHash, email, nickname, timeZone);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        protected async Task<QuestLabel> AddQuestLabelAsync(int userProfileId, string value, string backgroundColor)
        {
            var label = QuestLabel.Create(userProfileId, value, backgroundColor);
            _context.QuestLabels.Add(label);
            await _context.SaveChangesAsync();
            return label;
        }

        protected async Task<Friendship> AddFriendshipAsync(UserProfile userProfile1, UserProfile userProfile2)
        {
            var friendship = Friendship.Create(userProfile1.Id, userProfile2.Id, _fixedTestInstant.ToDateTimeUtc());
            _context.Friendships.Add(friendship);
            userProfile1.IncreaseFriendsCount();
            userProfile2.IncreaseFriendsCount();
            await _context.SaveChangesAsync();
            return friendship;
        }

        protected async Task<UserBlock> AddUserBlockAsync(int blockerUserProfileId, int blockedUserProfileId)
        {
            var userBlock = UserBlock.Create(blockerUserProfileId, blockedUserProfileId, _fixedTestInstant.ToDateTimeUtc());
            _context.UserBlocks.Add(userBlock);
            await _context.SaveChangesAsync();
            return userBlock;
        }

        protected async Task<FriendInvitation> AddFriendInvitationAsync(int senderUserProfileId, int receiverUserProfileId)
        {
            var friendInvitation = FriendInvitation.Create(senderUserProfileId, receiverUserProfileId, _fixedTestInstant.ToDateTimeUtc());
            _context.FriendInvitations.Add(friendInvitation);
            await _context.SaveChangesAsync();
            return friendInvitation;
        }

        protected async Task<UserInventory> AddUserInventoryItemAsync(int userProfileId, int shopItemId, int quantity)
        {
            var inventoryItem = UserInventory.Create(userProfileId, shopItemId, quantity, _fixedTestInstant.ToDateTimeUtc());
            _context.UserInventories.Add(inventoryItem);
            await _context.SaveChangesAsync();
            return inventoryItem;
        }

        protected async Task<Account> CreateUserWithLevelAndCoins(int level, int coins)
        {
            var user = await AddAccountAsync($"user_lvl{level}_{coins}coins@test.com", "password", $"User_Lvl{level}_{coins}Coins");
            user.Profile.TotalXp = level * 100; // Based on TestBase mock: 1 level per 100 XP
            user.Profile.Coins = coins;
            await _unitOfWork.SaveChangesAsync();
            return user;
        }
    }
}
