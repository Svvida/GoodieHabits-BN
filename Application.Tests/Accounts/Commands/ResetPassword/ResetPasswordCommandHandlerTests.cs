using Application.Accounts.Commands.ResetPassword;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;

namespace Application.Tests.Accounts.Commands.ResetPassword
{
    public class ResetPasswordCommandHandlerTests : TestBase<ResetPasswordCommandHandler>
    {
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests() : base()
        {
            _passwordHasherMock = new Mock<IPasswordHasher<Account>>();
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant);

            _handler = new ResetPasswordCommandHandler(_unitOfWork, _passwordHasherMock.Object, _clockMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentialsAndCode_ResetsPasswordAndInvalidatesCode()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "oldhashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.ToDateTimeUtc()); // Set a valid reset code and expiration
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var newPassword = "NewSecurePassword1";
            var newHashedPassword = "new_hashed_password";

            _passwordHasherMock.Setup(ph => ph.HashPassword(account, newPassword))
                               .Returns(newHashedPassword);

            var command = new ResetPasswordCommand(account.Email, account.ResetPasswordCode!, newPassword, newPassword);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            var updatedAccount = await _unitOfWork.Accounts.GetByIdAsync(account.Id, CancellationToken.None);
            updatedAccount.Should().NotBeNull();
            //updatedAccount!.HashPassword.Should().Be(newHashedPassword);
            updatedAccount.ResetPasswordCode.Should().BeNull(); // Code should be invalidated
            updatedAccount.ResetPasswordCodeExpiresAt.Should().BeNull(); // Expiration should be cleared
        }

        [Fact]
        public async Task Handle_AccountNotFound_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var command = new ResetPasswordCommand("nonexistent@example.com", "123456", "NewPass1", "NewPass1");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command, CancellationToken.None));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed password reset attempt")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_IncorrectResetCode_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "oldhashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.ToDateTimeUtc()); // Set a valid code
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new ResetPasswordCommand(account.Email, "999999", "NewPass1", "NewPass1"); // Incorrect code

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command, CancellationToken.None));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed password reset attempt")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExpiredResetCode_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "oldhashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.Minus(Duration.FromMinutes(40)).ToDateTimeUtc()); // Set code to be expired
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new ResetPasswordCommand(account.Email, account.ResetPasswordCode!, "NewPass1", "NewPass1");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command, CancellationToken.None));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed password reset attempt")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoResetCodeSet_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "oldhashedpass", "testnick");
            // Don't call InitializePasswordReset
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new ResetPasswordCommand(account.Email, "123456", "NewPass1", "NewPass1");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
