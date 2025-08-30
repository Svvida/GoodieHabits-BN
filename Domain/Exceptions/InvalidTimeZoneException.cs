namespace Domain.Exceptions
{
    public class InvalidTimeZoneException(int userId, string timeZone) : AppException($"Invalid time zone {timeZone} for user {userId}.", 400)
    {
    }
}
