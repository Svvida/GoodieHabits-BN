namespace Domain.Exceptions
{
    public class FailedToGenerateResetPasswordCodeException(int accountId) : AppException($"Failed to generate reset password code for account with ID {accountId}.", 500)
    {
        public int AccountId { get; } = accountId;
    }
}
