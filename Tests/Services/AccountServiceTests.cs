using System.Linq.Expressions;
using Application.Dtos.Accounts;
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
        private readonly Mock<IQuestLabelRepository> _questLabelRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        public AccountServiceTests()
        {
            _accountService = new AccountService(
                _accountRepositoryMock.Object,
                _mapperMock.Object,
                _userProfileRepositoryMock.Object,
                _passwordHasherMock.Object,
                _questLabelRepositoryMock.Object);
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
                UserXp = account.Profile.TotalXp,
                Level = 1, // Or whatever the expected level is for the test TotalXp
                NextLevelTotalXpRequired = 100, // Or expected value
                IsMaxLevel = false,
                CompletedQuests = account.Profile.CompletedQuests,
                TotalQuests = account.Profile.TotalQuests,
                JoinDate = account.CreatedAt
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()))
                .ReturnsAsync(account);

            _mapperMock.Setup(m => m.Map<GetAccountDto>(account))
                .Returns(expectedDto);

            // Act
            var result = await _accountService.GetAccountByIdAsync(1);

            // Assert
            _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<GetAccountDto>(account), Times.Once);
            result.Should().BeSameAs(expectedDto);
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
                .Setup(repo => repo.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _accountRepositoryMock
                .Setup(repo => repo.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userProfileRepositoryMock
                .Setup(repo => repo.DoesNicknameExistAsync(updateDto.Nickname, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mapperMock
                .Setup(m => m.Map(updateDto, account));

            // Act
            await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Account, object>>[]>()), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()), Times.Once);
            _userProfileRepositoryMock.Verify(repo => repo.DoesNicknameExistAsync(updateDto.Nickname, It.IsAny<CancellationToken>()), Times.Once);

            _mapperMock.Verify(m => m.Map(updateDto, account), Times.Once);

            _accountRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Account>(a => a == account), It.IsAny<CancellationToken>()), Times.Once);
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
                .Setup(repo => repo.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()))
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
                .Setup(repo => repo.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()))
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
                .Setup(repo => repo.DoesNicknameExistAsync(updateDto.Nickname, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage($"Nickname {updateDto.Nickname} is already in use");
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

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
        {
            // Arrange
            const int accountId = 1;
            const string correctOldPasswordHash = "hashed_old_password";
            var account = AccountFactory.CreateAccount(accountId, "test@email.com", correctOldPasswordHash);
            var deleteAccountDto = new DeleteAccountDto
            {
                Password = "wrongOldPassword"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password))
                .Returns(PasswordVerificationResult.Failed);

            // act
            Func<Task> act = async () => await _accountService.DeleteAccountAsync(accountId, deleteAccountDto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid password");

            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password), Times.Once);
            _questLabelRepositoryMock.Verify(repo => repo.DeleteQuestLabelsByAccountIdAsync(accountId, CancellationToken.None), Times.Never);
            _accountRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Account>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldDeleteAccountAndQuestLabels()
        {
            // Arrange
            const int accountId = 1;
            const string correctOldPasswordHash = "hashed_old_password";
            var account = AccountFactory.CreateAccount(accountId, "test@email.com", correctOldPasswordHash);
            var deleteAccountDto = new DeleteAccountDto
            {
                Password = "correctOldPassword"
            };

            _accountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password))
                .Returns(PasswordVerificationResult.Success);

            _accountRepositoryMock
                .Setup(repo => repo.DeleteAsync(account, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _questLabelRepositoryMock
                .Setup(repo => repo.DeleteQuestLabelsByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _accountService.DeleteAccountAsync(accountId, deleteAccountDto, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _questLabelRepositoryMock.Verify(repo => repo.DeleteQuestLabelsByAccountIdAsync(accountId, CancellationToken.None), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.DeleteAsync(account, CancellationToken.None), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password), Times.Once);

            _accountRepositoryMock.Verify();
            _questLabelRepositoryMock.Verify();
        }
    }
}
