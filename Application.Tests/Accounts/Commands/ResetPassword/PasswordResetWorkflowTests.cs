using Application.Accounts.Commands.RequestPasswordReset;
using Application.Accounts.Commands.ResetPassword;
using Application.Accounts.Commands.VerifyPasswordResetCode;
using Application.Common.Interfaces.Email;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;

namespace Application.Tests.Accounts.Commands.ResetPassword
{
    public class PasswordResetWorkflowTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<IForgotPasswordEmailSender> _emailSenderMock;
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock;
        private readonly Mock<ILogger<RequestPasswordResetCommandHandler>> _requestLoggerMock;
        private readonly Mock<ILogger<ResetPasswordCommandHandler>> _resetLoggerMock;

        private readonly RequestPasswordResetCommandHandler _requestHandler;
        private readonly VerifyPasswordResetCodeCommandHandler _verifyHandler;
        private readonly ResetPasswordCommandHandler _resetHandler;

        private readonly Instant _fixedTestInstant;


        public PasswordResetWorkflowTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

            _unitOfWork = new UnitOfWork(_dbContext);

            _clockMock = new Mock<IClock>();
            _emailSenderMock = new Mock<IForgotPasswordEmailSender>();
            _passwordHasherMock = new Mock<IPasswordHasher<Account>>();
            _requestLoggerMock = new Mock<ILogger<RequestPasswordResetCommandHandler>>();
            _resetLoggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();

            // Setup clock for consistent time across workflow steps
            _fixedTestInstant = Instant.FromUtc(2023, 10, 26, 10, 0, 0);
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant);

            // Setup password hasher for consistent hashing during reset
            _passwordHasherMock.Setup(ph => ph.HashPassword(It.IsAny<Account>(), It.IsAny<string>()))
                               .Returns((Account acc, string newPass) => "hashed_" + newPass); // Simple mock hashing

            // Instantiate all handlers
            _requestHandler = new RequestPasswordResetCommandHandler(_unitOfWork, _emailSenderMock.Object, _clockMock.Object);
            _verifyHandler = new VerifyPasswordResetCodeCommandHandler(_unitOfWork, _clockMock.Object);
            _resetHandler = new ResetPasswordCommandHandler(_unitOfWork, _passwordHasherMock.Object, _clockMock.Object, _resetLoggerMock.Object);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        // Helper to add account
        private async Task<Account> AddAccountAsync(string email, string passwordHash, string nickname, string timeZone = "Etc/UTC")
        {
            var account = Account.Create(passwordHash, email, nickname, timeZone);
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        [Fact]
        public async Task CompletePasswordResetWorkflow_Success()
        {
            // Arrange
            var userEmail = "workflow@example.com";
            var oldPasswordHash = "initial_hashed_password";
            var newPassword = "MySuperNewPassword1";

            var account = await AddAccountAsync(userEmail, oldPasswordHash, "workflow_user");
            var accountId = account.Id;

            // Step 1: Request Password Reset
            var requestCommand = new RequestPasswordResetCommand(userEmail);
            await _requestHandler.Handle(requestCommand, CancellationToken.None);

            var accountAfterRequest = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            accountAfterRequest.Should().NotBeNull();
            accountAfterRequest!.ResetPasswordCode.Should().NotBeNullOrEmpty();
            accountAfterRequest.ResetPasswordCodeExpiresAt.Should().Be(_fixedTestInstant.Plus(Duration.FromMinutes(15)).ToDateTimeUtc());

            _emailSenderMock.Verify(es => es.SendForgotPasswordEmailAsync(
                userEmail, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            // Advance time for verification
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(_fixedTestInstant.Plus(Duration.FromMinutes(5)));

            // Step 2: Verify Password Reset Code
            var verifyCommand = new VerifyPasswordResetCodeCommand(userEmail, accountAfterRequest.ResetPasswordCode!);
            var verifyResult = await _verifyHandler.Handle(verifyCommand, CancellationToken.None);
            verifyResult.Should().BeTrue();

            // Step 3: Reset Password
            var resetCommand = new ResetPasswordCommand(userEmail, accountAfterRequest.ResetPasswordCode!, newPassword, newPassword);
            await _resetHandler.Handle(resetCommand, CancellationToken.None);

            // Assert Final State
            var finalAccount = await _unitOfWork.Accounts.GetByIdAsync(accountId, CancellationToken.None);
            finalAccount.Should().NotBeNull();
            finalAccount!.HashPassword.Should().Be("hashed_" + newPassword); // Based on mock hashing
            finalAccount.ResetPasswordCode.Should().BeNull();
            finalAccount.ResetPasswordCodeExpiresAt.Should().BeNull();

            // No warning logs should be present for a successful flow
            _resetLoggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}