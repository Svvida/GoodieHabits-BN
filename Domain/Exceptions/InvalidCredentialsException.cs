namespace Domain.Exceptions
{
    public class InvalidCredentialsException(string message) : AppException(message, 400)
    {
    }
}
