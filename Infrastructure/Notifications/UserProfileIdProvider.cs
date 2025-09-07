using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Notifications
{
    public class UserProfileIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("userProfileId")?.Value;
        }
    }
}