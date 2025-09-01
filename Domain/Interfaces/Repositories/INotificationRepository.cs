using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        public Task<Notification?> GetUserNotificationByIdAsync(int notificationId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Notification>> GetAllUserNotifications(int userProfileId, bool onlyUnread, CancellationToken cancellationToken = default);
        public Task<bool> IsUserNotificationExistsAsync(int notificationId, int userProfileId, CancellationToken cancellationToken = default);
        public Task MarkUserNotificationAsReadAsync(int notificationId, int userProfileId, CancellationToken cancellationToken = default);
        public Task<int> MarkAllUserNotificationsAsReadAsync(int userProfileId, CancellationToken cancellationToken = default);
    }
}
