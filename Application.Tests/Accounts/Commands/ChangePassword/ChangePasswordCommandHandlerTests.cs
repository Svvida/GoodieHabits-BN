using Application.Accounts.Commands.ChangePassword;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommandHandlerTests : TestBase<ChangePasswordCommandHandler>
    {
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock;
        private readonly ChangePasswordCommandHandler _handler;

        public ChangePasswordCommandHandlerTests() : base()
        {
            _passwordHasherMock = new Mock<IPasswordHasher<Account>>();
            // Note: We don't need _mediatorMock or _loggerMock for this specific handler as it doesn't use them.
            _handler = new ChangePasswordCommandHandler(_unitOfWork, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ChangesPassword()
        {
            // Arrange
            var initialHash = "hashed_old_password";
            var oldPassword = "OldSecurePassword1";
            var newPassword = "NewSecurePassword1";
            var newHash = "hashed_new_password"; // This will be returned by the hasher mock

            var account = Account.Create(initialHash, "test@example.com", "TestUser");
            await _unitOfWork.Accounts.AddAsync(account, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            // Configure password hasher mock
            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, oldPassword))
                               .Returns(PasswordVerificationResult.Success);
            _passwordHasherMock.Setup(ph => ph.HashPassword(account, newPassword))
                               .Returns(newHash);

            var command = new ChangePasswordCommand(oldPassword, newPassword, newPassword, account.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            var updatedAccount = await _unitOfWork.Accounts.GetByIdAsync(account.Id, CancellationToken.None);
            updatedAccount.Should().NotBeNull();
            updatedAccount!.HashPassword.Should().Be(newHash); // Verify the password was updated
            updatedAccount.HashPassword.Should().NotBe(initialHash); // Ensure it's different from old
        }

        [Fact]
        public async Task Handle_AccountNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            var command = new ChangePasswordCommand("OldPass1", "NewPass1", "NewPass1", 999);

            // We don't need to setup VerifyHashedPassword as the account won't be found
            // The default for Mock is to return null for GetByIdAsync if not explicitly setup.

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify that HashPassword was never called since the account wasn't found
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidOldPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var initialHash = "hashed_old_password";
            var incorrectOldPassword = "WrongOldPassword!";
            var newPassword = "NewSecurePassword1";

            var account = Account.Create(initialHash, "test@example.com", "TestUser");
            await _unitOfWork.Accounts.AddAsync(account, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            // Setup mock to return Failed for the incorrect password
            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, incorrectOldPassword))
                               .Returns(PasswordVerificationResult.Failed);

            var command = new ChangePasswordCommand(incorrectOldPassword, newPassword, newPassword, account.Id);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify that HashPassword was never called as verification failed
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);

            // Verify the account's password hash was NOT changed
            var unchangedAccount = await _unitOfWork.Accounts.GetByIdAsync(account.Id, CancellationToken.None);
            unchangedAccount!.HashPassword.Should().Be(initialHash);
        }

        [Fact]
        public async Task Handle_PasswordHasherReturnsFailedForAccount_ThrowsUnauthorizedException()
        {
            // Arrange
            var initialHash = "hashed_old_password";
            var oldPassword = "OldSecurePassword1";
            var newPassword = "NewSecurePassword1";

            var account = Account.Create("test@example.com", initialHash, "TestUser");
            await _unitOfWork.Accounts.AddAsync(account, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            // Setup mock to return Failed even if technically the old password might be 'correct' from command perspective,
            // this tests the integration with a hasher that deems it failed.
            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, oldPassword))
                               .Returns(PasswordVerificationResult.Failed);

            var command = new ChangePasswordCommand(oldPassword, newPassword, newPassword, account.Id);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify that HashPassword was never called
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
        }
    }
}
