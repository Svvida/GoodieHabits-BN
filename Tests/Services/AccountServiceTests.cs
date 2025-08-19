using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Factories;

namespace Tests.Services
{
    public class AccountServiceTests
    {
        private readonly AccountService _accountService;
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<AccountService>> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public AccountServiceTests()
        {
            _accountService = new AccountService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _passwordHasherMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
        {
            // Arrange
            var account = AccountFactory.CreateAccountWithProfile(1, "test@email.com");
            var expectedDto = new GetAccountWithProfileDto
            {
                Login = account.Login,
                Email = account.Email
            };

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _mapperMock.Setup(m => m.Map<GetAccountWithProfileDto>(account))
                .Returns(expectedDto);

            // Act
            var result = await _accountService.GetAccountWithProfileInfoAsync(1);

            // Assert
            _unitOfWorkMock.Verify(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<GetAccountWithProfileDto>(account), Times.Once);
            result.Should().BeSameAs(expectedDto);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
        {
            // Arrange
            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account?)null);

            // Act
            var act = async () => await _accountService.GetAccountWithProfileInfoAsync(1);

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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _unitOfWorkMock
                .Setup(repo => repo.UserProfiles.DoesNicknameExistAsync(updateDto.Nickname, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mapperMock
                .Setup(m => m.Map(updateDto, account));

            // Act
            await _accountService.UpdateAccountAsync(1, updateDto);

            // Assert
            _unitOfWorkMock.Verify(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(repo => repo.Accounts.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(repo => repo.Accounts.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(repo => repo.UserProfiles.DoesNicknameExistAsync(updateDto.Nickname, 1, It.IsAny<CancellationToken>()), Times.Once);

            _mapperMock.Verify(m => m.Map(updateDto, account), Times.Once);

            _unitOfWorkMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.DoesLoginExistAsync(updateDto.Login, 1, It.IsAny<CancellationToken>()))
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.DoesEmailExistAsync(updateDto.Email, 1, It.IsAny<CancellationToken>()))
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _unitOfWorkMock
                .Setup(repo => repo.UserProfiles.DoesNicknameExistAsync(updateDto.Nickname, 1, It.IsAny<CancellationToken>()))
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, changePasswordDto.OldPassword))
                .Returns(PasswordVerificationResult.Success);

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(account, changePasswordDto.NewPassword))
                .Returns(newlyHashedPassword);

            // Act
            await _accountService.ChangePasswordAsync(accountId, changePasswordDto, CancellationToken.None);

            // Assert
            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, initialHashedPassword, oldPassword), Times.Once);

            _passwordHasherMock.Verify(hasher => hasher.HashPassword(account, newPassword), Times.Once);

            account.HashPassword.Should().Be(newlyHashedPassword);
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
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
            _unitOfWorkMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(nonExistentAccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account?)null);

            // Act
            Func<Task> act = async () => await _accountService.ChangePasswordAsync(nonExistentAccountId, changePasswordDto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Account with ID: {nonExistentAccountId} not found");

            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
        {
            // Arrange
            const int accountId = 1;
            const string correctOldPasswordHash = "hashed_old_password";
            var account = AccountFactory.CreateAccount(accountId, "test@email.com", correctOldPasswordHash);
            var deleteAccountDto = new PasswordConfirmationDto
            {
                Password = "wrongOldPassword",
                ConfirmPassword = "wrongOldPassword"
            };

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
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
            _unitOfWorkMock.Verify(repo => repo.Accounts.Remove(account), Times.Never);
            _unitOfWorkMock.Verify(repo => repo.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldDeleteAccountAndQuestLabels()
        {
            // Arrange
            const int accountId = 1;
            const string correctOldPasswordHash = "hashed_old_password";
            var account = AccountFactory.CreateAccount(accountId, "test@email.com", correctOldPasswordHash);
            var deleteAccountDto = new PasswordConfirmationDto
            {
                Password = "correctOldPassword",
                ConfirmPassword = "correctOldPassword"
            };

            _unitOfWorkMock
                .Setup(repo => repo.Accounts.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password))
                .Returns(PasswordVerificationResult.Success);

            // Act
            Func<Task> act = async () => await _accountService.DeleteAccountAsync(accountId, deleteAccountDto, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _unitOfWorkMock.Verify(repo => repo.Accounts.Remove(account), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password), Times.Once);

            _unitOfWorkMock.Verify();
        }
    }
}
