using Application.Accounts.Commands.UpdateAccount;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandHandlerTests : TestBase<UpdateAccountCommandHandler>
    {
        public UpdateAccountCommandHandlerTests() : base()
        {
        }

        [Fact]
        public async Task Handle_ExistingAccounts_UpdatesAccountDetails()
        {
            // Arrange
            var account = Account.Create("hashed_password", "test@example.com", "OldNickname");
            account.Profile.UpdateBio("Old bio");
            account.UpdateLogin("OldLogin");
            account.UpdateEmail("old@example.com");

            await _context.Accounts.AddAsync(account, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new UpdateAccountCommandHandler(_unitOfWork, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: account.Id,
                UserProfileId: account.Profile.Id,
                Login: "newlogin",
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "new bio text"
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedAccount = await _unitOfWork.Accounts.GetAccountWithProfileAsync(account.Id, CancellationToken.None);
            updatedAccount.Should().NotBeNull();
            updatedAccount!.Login.Should().Be("newlogin");
            updatedAccount.Email.Should().Be("new@example.com");
            updatedAccount.Profile.Nickname.Should().Be("newnick");
            updatedAccount.Profile.Bio.Should().Be("new bio text");

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Account")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingAccount_ThrowsNotFoundException()
        {
            // Arrange
            var handler = new UpdateAccountCommandHandler(_unitOfWork, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: 1, // Non-existent ID
                UserProfileId: 2,
                Login: "newlogin",
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "new bio text"
            );

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdatesWithSameValues_DoesNotThrowAndPersists()
        {
            // Arrange
            var account = Account.Create("hashed_password", "test@example.com", "Nick");
            account.Profile.UpdateBio("Bio");
            account.UpdateLogin("login");
            account.UpdateEmail("test@example.com");

            await _context.Accounts.AddAsync(account, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new UpdateAccountCommandHandler(_unitOfWork, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: account.Id,
                UserProfileId: account.Profile.Id,
                Login: "login",
                Email: "test@example.com",
                Nickname: "Nick",
                Bio: "Bio"
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedAccount = await _unitOfWork.Accounts.GetAccountWithProfileAsync(account.Id, CancellationToken.None);
            updatedAccount.Should().NotBeNull();
            updatedAccount!.Login.Should().Be("login");
            updatedAccount.Email.Should().Be("test@example.com");
            updatedAccount.Profile.Nickname.Should().Be("Nick");
            updatedAccount.Profile.Bio.Should().Be("Bio");
        }

        [Fact]
        public async Task Handle_UpdatesWithNullBio_SetsBioToNull()
        {
            // Arrange
            var account = Account.Create("hashed_password", "test@example.com", "Nick");
            account.Profile.UpdateBio("Old bio");
            account.UpdateLogin("login");
            account.UpdateEmail("test@example.com");

            await _context.Accounts.AddAsync(account, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new UpdateAccountCommandHandler(_unitOfWork, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: account.Id,
                UserProfileId: account.Profile.Id,
                Login: "login",
                Email: "test@example.com",
                Nickname: "Nick",
                Bio: null
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedAccount = await _unitOfWork.Accounts.GetAccountWithProfileAsync(account.Id, CancellationToken.None);
            updatedAccount!.Profile.Bio.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsSaveChangesAsyncOnce()
        {
            // Arrange
            var account = Account.Create("hashed_password", "test@example.com", "Nick");
            account.Profile.UpdateBio("Bio");
            account.UpdateLogin("login");
            account.UpdateEmail("test@example.com");

            await _context.Accounts.AddAsync(account, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(r => r.GetAccountWithProfileAsync(account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);
            unitOfWorkMock.Setup(u => u.Accounts).Returns(accountRepoMock.Object);

            var handler = new UpdateAccountCommandHandler(unitOfWorkMock.Object, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: account.Id,
                UserProfileId: account.Profile.Id,
                Login: "newlogin",
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "new bio"
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_LogsDebugMessageWithCorrectPayload()
        {
            // Arrange
            var account = Account.Create("hashed_password", "test@example.com", "Nick");
            account.Profile.UpdateBio("Bio");
            account.UpdateLogin("login");
            account.UpdateEmail("test@example.com");

            await _context.Accounts.AddAsync(account, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new UpdateAccountCommandHandler(_unitOfWork, _loggerMock.Object);
            var command = new UpdateAccountCommand(
                AccountId: account.Id,
                UserProfileId: account.Profile.Id,
                Login: "newlogin",
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "new bio"
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Account") && v.ToString().Contains(command.AccountId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}