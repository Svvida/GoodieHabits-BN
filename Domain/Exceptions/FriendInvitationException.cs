namespace Domain.Exceptions
{
    public class FriendInvitationException(string message) : AppException(message, 409)
    {
    }
}
