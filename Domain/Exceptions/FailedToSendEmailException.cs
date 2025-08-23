namespace Domain.Exceptions
{
    public class FailedToSendEmailException() : AppException("Failed to send the email.", 500)
    {
    }
}
