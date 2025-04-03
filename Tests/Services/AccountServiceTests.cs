using System.Linq.Expressions;
using Application.Dtos.Accounts;
using Application.MappingProfiles;
using Application.Services;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;
using Tests.Factories;

namespace Tests.Services
{
    public class AccountServiceTests
    {
        private readonly AccountService _accountService;
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock = new();
        private readonly IMapper _realMapper;

        public AccountServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AccountProfile());
            });
            _realMapper = config.CreateMapper();

            _accountService = new AccountService(
                _accountRepositoryMock.Object,
                _realMapper,
                _userProfileRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "password", "test@email.com", "Etc/UTC");
            var expectedDto = new GetAccountDto
            {
                Login = account.Login,
                Email = account.Email,
                Nickname = account.Profile.Nickname,
                Bio = account.Profile.Bio,
                Level = 1,
                CompletedQuests = 0,
                TotalQuests = 0,
                Xp = 0,
                TotalXP = 0
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
        {
            // Arrange
            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync((Account?)null);

            // Act
            var act = async () => await _accountService.GetAccountByIdAsync(1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Account with ID 1 was not found");
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldUpdateAccount_WhenValid()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashedpwd", "test@email.com", "Etc/UTC");
            var updateDto = new UpdateAccountDto
            {
                Login = "NewLogin",
                Email = "new@email.com",
                Nickname = "NewNickname"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(repo => repo.ExistsByFieldAsync(a => a.Login, updateDto.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _accountRepositoryMock
                .Setup(repo => repo.ExistsByFieldAsync(a => a.Email, updateDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userProfileRepositoryMock
                .Setup(repo => repo.ExistsByNicknameAsync(updateDto.Nickname, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            _accountRepositoryMock.Verify(repo => repo.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldThrowConflictExeption_WhenLoginExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashpwd", "test@email.com", "Etc/Utc");
            var updateDto = new UpdateAccountDto
            {
                Login = "NewLogin"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(repo => repo.ExistsByFieldAsync(a => a.Login, updateDto.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage($"Login {updateDto.Login} is already in use");
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldThrowConflictException_WhenEmailExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashedpwd", "test@email.com", "Etc/UTC");
            var updateDto = new UpdateAccountDto
            {
                Email = "existing@email.com"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(repo => repo.ExistsByFieldAsync(a => a.Email, updateDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage($"Email {updateDto.Email} is already in use");
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldThrowConflictException_WhenNicknameExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashedpwd", "test@email.com", "Etc/UTC");
            var updateDto = new UpdateAccountDto
            {
                Nickname = "ExistingNickname"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            _userProfileRepositoryMock
                .Setup(repo => repo.ExistsByNicknameAsync(updateDto.Nickname, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage($"Nickname {updateDto.Nickname} is already in use");
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldClearFields_WhenNotProvidedInUpdateDto()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashedPwd", "test@email.com", "Etc/UTC");

            // CreateDto without Nickname & Bio
            var updateDto = new UpdateAccountDto
            {
                Login = "NewLogin",
                Email = "new@email.com"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            // Act
            await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert: Nickname and Bio should be null
            account.Profile.Nickname.Should().BeNull();
            account.Profile.Bio.Should().BeNull();
            account.Login.Should().Be(updateDto.Login);
            account.Email.Should().Be(updateDto.Email);
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldUpdateFileds_WhenProvidedInUpdateDto()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "hashedPwd", "test@email.com", "Etc/UTC");

            var updateDto = new UpdateAccountDto
            {
                Login = "UpdatedLogin",
                Email = "updated@email.com",
                Nickname = "UpdatedNick",
                Bio = "Updated Bio"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            // Act
            await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            account.Login.Should().Be(updateDto.Login);
            account.Email.Should().Be(updateDto.Email);
            account.Profile.Nickname.Should().Be(updateDto.Nickname);
            account.Profile.Bio.Should().Be(updateDto.Bio);
        }
    }
}
