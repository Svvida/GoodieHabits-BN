using Application.Accounts.Commands.VerifyPasswordResetCode;
using FluentAssertions;
using NodaTime;

namespace Application.Tests.Accounts.Commands.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeCommandHandlerTests : TestBase<VerifyPasswordResetCodeCommandHandler>
    {
        private readonly VerifyPasswordResetCodeCommandHandler _handler;

        public VerifyPasswordResetCodeCommandHandlerTests() : base()
        {
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant);

            _handler = new VerifyPasswordResetCodeCommandHandler(_unitOfWork, _clockMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCodeAndNotExpired_ReturnsTrue()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.ToDateTimeUtc()); // Set code and expiration
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new VerifyPasswordResetCodeCommand(account.Email, account.ResetPasswordCode!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AccountNotFound_ReturnsFalse()
        {
            // Arrange
            var command = new VerifyPasswordResetCodeCommand("nonexistent@example.com", "123456");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NoResetCodeSet_ReturnsFalse()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");
            // Don't call InitializePasswordReset, so ResetPasswordCode is null
            await _unitOfWork.SaveChangesAsync(CancellationToken.None); // Save account without reset code

            var command = new VerifyPasswordResetCodeCommand(account.Email, "123456");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExpiredResetCode_ReturnsFalse()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.Minus(Duration.FromMinutes(30)).ToDateTimeUtc()); // Set code to be expired
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new VerifyPasswordResetCodeCommand(account.Email, account.ResetPasswordCode!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_MismatchedResetCode_ReturnsFalse()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.ToDateTimeUtc()); // Set a valid code
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new VerifyPasswordResetCodeCommand(account.Email, "999999"); // Incorrect code

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}
