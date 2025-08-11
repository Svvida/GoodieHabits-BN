namespace Domain.Exceptions
{
    public class NotFoundException(string message) : AppException(message, 404)
    {
    }
}
