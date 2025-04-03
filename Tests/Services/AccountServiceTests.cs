using System.Linq.Expressions;
using Application.Dtos.Accounts;
using Application.MappingProfiles;
using Application.Services;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Tests.Factories;

namespace Tests.Services
{
    public class AccountServiceTests
    {
        private readonly AccountService _accountService;
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock = new();
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock = new();
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
                _userProfileRepositoryMock.Object,
                _passwordHasherMock.Object);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");

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
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");

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

        [Fact]
        public async Task ChangePasswordAsync_ShouldChangePassword_WhenOldPasswordIsValid()
        {
            // Arrange
            int accountId = 1;
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            string initialHashedPassword = "hashed_old_password";
            string newlyHashedPassword = "hashed_new_password";

            var account = AccountFactory.CreateAccount(accountId, "test@email.com", initialHashedPassword);
            var changePasswordDto = new ChangePasswordDto
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, changePasswordDto.OldPassword))
                .Returns(PasswordVerificationResult.Success);

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(account, changePasswordDto.NewPassword))
                .Returns(newlyHashedPassword);

            _accountRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _accountService.ChangePasswordAsync(accountId, changePasswordDto, CancellationToken.None);

            // Assert
            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, initialHashedPassword, oldPassword), Times.Once);

            _passwordHasherMock.Verify(hasher => hasher.HashPassword(account, newPassword), Times.Once);

            account.HashPassword.Should().Be(newlyHashedPassword);

            _accountRepositoryMock.Verify(repo => repo.UpdateAsync(
                It.Is<Account>(a => a.Id == accountId && a.HashPassword == newlyHashedPassword),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldThrowUnauthorizedException_WhenOldPasswordIsInvalid()
        {
            // Arrange
            int accountId = 1;
            string correctOldPasswordHash = "hashed_old_password";
            var account = AccountFactory.CreateAccount(accountId, "test@email.com", correctOldPasswordHash);
            var changePasswordDto = new ChangePasswordDto
            {
                OldPassword = "wrongOldPassword",
                NewPassword = "newPassword"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, changePasswordDto.OldPassword))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            Func<Task> act = async () => await _accountService.ChangePasswordAsync(accountId, changePasswordDto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid password");

            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, correctOldPasswordHash, changePasswordDto.OldPassword), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
            _accountRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldThrowNotFoundException_WhenAccountDoesNotExists()
        {
            // Arrange
            int nonExistentAccountId = 999;
            var changePasswordDto = new ChangePasswordDto
            {
                OldPassword = "oldPassword",
                NewPassword = "newPassword"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(nonExistentAccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account?)null);

            // Act
            Func<Task> act = async () => await _accountService.ChangePasswordAsync(nonExistentAccountId, changePasswordDto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Account with ID: {nonExistentAccountId} not found");

            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
            _accountRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
