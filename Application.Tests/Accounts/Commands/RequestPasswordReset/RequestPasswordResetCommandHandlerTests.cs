using Application.Accounts.Commands.RequestPasswordReset;
using FluentAssertions;
using MediatR;
using Moq;
using NodaTime;

namespace Application.Tests.Accounts.Commands.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandlerTests : TestBase<RequestPasswordResetCommandHandler>
    {
        private readonly RequestPasswordResetCommandHandler _handler;

        public RequestPasswordResetCommandHandlerTests() : base()
        {
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant);

            _handler = new RequestPasswordResetCommandHandler(_unitOfWork, _emailSenderMock.Object, _clockMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingAccount_InitializesPasswordResetAndSendsEmail()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");

            var command = new RequestPasswordResetCommand(account.Email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            var updatedAccount = await _unitOfWork.Accounts.GetByIdAsync(account.Id, CancellationToken.None);
            updatedAccount.Should().NotBeNull();
            updatedAccount!.ResetPasswordCode.Should().NotBeNullOrEmpty().And.HaveLength(6).And.MatchRegex("^[0-9]{6}$");
            updatedAccount.ResetPasswordCodeExpiresAt.Should().Be(_fixedTestInstant.Plus(Duration.FromMinutes(15)).ToDateTimeUtc());

            _emailSenderMock.Verify(es => es.SendForgotPasswordEmailAsync(
                account.Email,
                It.Is<string>(code => code == updatedAccount.ResetPasswordCode),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingAccount_ReturnsSuccessAndDoesNotSendEmail()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@example.com";
            var command = new RequestPasswordResetCommand(nonExistentEmail);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value); // Important: returns success to prevent email enumeration

            // Verify no email was sent
            _emailSenderMock.Verify(es => es.SendForgotPasswordEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingAccount_ResetsExistingCode()
        {
            // Arrange
            var account = await AddAccountAsync("test@example.com", "hashedpass", "testnick");
            account.InitializePasswordReset(_fixedTestInstant.Minus(Duration.FromMinutes(30)).ToDateTimeUtc()); // Set an old code
            await _unitOfWork.SaveChangesAsync(CancellationToken.None); // Persist old code

            var oldCode = account.ResetPasswordCode;
            var command = new RequestPasswordResetCommand(account.Email);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedAccount = await _unitOfWork.Accounts.GetByIdAsync(account.Id, CancellationToken.None);
            updatedAccount!.ResetPasswordCode.Should().NotBe(oldCode); // Ensure new code is generated
            updatedAccount.ResetPasswordCodeExpiresAt.Should().Be(_fixedTestInstant.Plus(Duration.FromMinutes(15)).ToDateTimeUtc());
            _emailSenderMock.Verify(es => es.SendForgotPasswordEmailAsync(
                account.Email,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
