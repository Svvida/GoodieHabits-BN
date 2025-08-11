namespace Domain.Exceptions
{
    public class InvalidArgumentException(string message) : AppException(message, 400)
    {
    }
}
