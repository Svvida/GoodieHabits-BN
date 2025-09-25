using Application.Accounts.Commands.DeleteAccount;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.Tests.Accounts.Commands.DeleteAccount
{
    public class DeleteAccountCommandHandlerTests : TestBase<DeleteAccountCommandHandler>
    {
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock;
        private readonly DeleteAccountCommandHandler _handler;

        public DeleteAccountCommandHandlerTests() : base()
        {
            _passwordHasherMock = new Mock<IPasswordHasher<Account>>();
            _handler = new DeleteAccountCommandHandler(_unitOfWork, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentialsAndNoLabels_DeletesAccount()
        {
            // Arrange
            var initialHash = "hashed_password";
            var password = "CorrectPassword1";

            // Use the helper to create and save account; ID will be generated
            var account = await AddAccountAsync("test@example.com", initialHash, "testnick");
            var accountId = account.Id; // Get the generated ID
            var userProfileId = account.Profile.Id; // Get the generated UserProfileId

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, password))
                               .Returns(PasswordVerificationResult.Success);

            var command = new DeleteAccountCommand(password, password, accountId, userProfileId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            // Verify account is deleted
            var deletedAccount = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            deletedAccount.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidCredentialsAndExistingLabels_DeletesAccountAndLabels()
        {
            // Arrange
            var initialHash = "hashed_password";
            var password = "CorrectPassword1";

            var account = await AddAccountAsync("test@example.com", initialHash, "testnick");
            var accountId = account.Id;
            var userProfileId = account.Profile.Id; // Get the generated UserProfileId

            // Add labels using the generated UserProfileId
            var label1 = await AddQuestLabelAsync(userProfileId, "Work", "#FF0000");
            var label2 = await AddQuestLabelAsync(userProfileId, "Home", "#00FF00");

            // Assuming your GetUserLabelsAsync specifically queries by UserProfileId derived from AccountId,
            // and doesn't have an `IsRemovable` filter. If it does, make sure the labels match.
            // For simplicity, I'm assuming labels created here will be returned by GetUserLabelsAsync.

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, password))
                               .Returns(PasswordVerificationResult.Success);

            var command = new DeleteAccountCommand(password, password, accountId, userProfileId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            // Verify account is deleted
            var deletedAccount = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            deletedAccount.Should().BeNull();

            // Verify labels are deleted.
            // This is tricky if GetUserLabelsAsync internally relies on the Account's UserProfileId.
            // After account deletion, the profile might also be gone depending on cascade behavior.
            // A more robust way to check: directly query the DbContext.
            var remainingLabelsInDb = await _context.QuestLabels
                                                      .Where(ql => ql.UserProfileId == userProfileId)
                                                      .ToListAsync(CancellationToken.None);
            remainingLabelsInDb.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_AccountNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            var nonExistentAccountId = 999; // An ID that won't exist
            var nonExistentUserProfileId = 999; // Corresponding non-existent UserProfileId
            var command = new DeleteAccountCommand("Password123", "Password123", nonExistentAccountId, nonExistentUserProfileId);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify no interactions with password hasher
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
            _passwordHasherMock.Verify(ph => ph.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var initialHash = "hashed_password";
            var correctPassword = "CorrectPassword1";
            var incorrectPassword = "WrongPassword!";

            var account = await AddAccountAsync("test@example.com", initialHash, "testnick");
            var accountId = account.Id;
            var userProfileId = account.Profile.Id;

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, incorrectPassword))
                               .Returns(PasswordVerificationResult.Failed);

            var command = new DeleteAccountCommand(incorrectPassword, incorrectPassword, accountId, userProfileId);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify account was NOT deleted
            var existingAccount = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            existingAccount.Should().NotBeNull();

            // Verify no other repo methods were called that would modify state
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCredentialsButNoLabels_RemovesAccountOnly()
        {
            // Arrange
            var initialHash = "hashed_password";
            var password = "CorrectPassword1";

            var account = await AddAccountAsync("test@example.com", initialHash, "testnick");
            var accountId = account.Id;
            var userProfileId = account.Profile.Id;
            // No labels added for this account

            _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(account, initialHash, password))
                               .Returns(PasswordVerificationResult.Success);

            var command = new DeleteAccountCommand(password, password, accountId, userProfileId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            var deletedAccount = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            deletedAccount.Should().BeNull();
        }
    }
}
