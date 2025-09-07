using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        public Task<Notification?> GetUserNotificationByIdAsync(Guid notificationId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Notification>> GetAllUserNotifications(int userProfileId, bool onlyUnread, CancellationToken cancellationToken = default);
        public Task<bool> IsUserNotificationExistsAsync(Guid notificationId, int userProfileId, CancellationToken cancellationToken = default);
        public Task MarkUserNotificationAsReadAsync(Guid notificationId, int userProfileId, CancellationToken cancellationToken = default);
        public Task<int> MarkAllUserNotificationsAsReadAsync(int userProfileId, CancellationToken cancellationToken = default);
    }
}
