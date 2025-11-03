namespace Domain.Exceptions
{
    public class FriendshipException(string message) : AppException(message, 409)
    {
    }
}
