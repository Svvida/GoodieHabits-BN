using Application.Common.Interfaces.Email;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
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

            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<THandler>>();
            _clockMock = new Mock<IClock>();
            _emailSenderMock = new Mock<IForgotPasswordEmailSender>();
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

        protected async Task<Friendship> AddFriendshipAsync(int userProfileId1, int userProfileId2)
        {
            var friendship = Friendship.Create(userProfileId1, userProfileId2, _fixedTestInstant.ToDateTimeUtc());
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }
    }
}
