using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository(AppDbContext context) : BaseRepository<Notification>(context), INotificationRepository
    {
        public async Task<Notification?> GetUserNotificationByIdAsync(Guid notificationId, int userProfileId, bool asNoTracking = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications.Where(n => n.Id == notificationId && n.UserProfileId == userProfileId);

            if (asNoTracking)
                query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<IEnumerable<Notification>> GetAllUserNotifications(int userProfileId, bool onlyUnread, CancellationToken cancellationToken = default)
        {
            if (onlyUnread)
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .OrderByDescending(n => n.CreatedAt)
                    .Where(n => n.UserProfileId == userProfileId && !n.IsRead)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .OrderByDescending(n => n.CreatedAt)
                    .Where(n => n.UserProfileId == userProfileId)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        public async Task<bool> IsUserNotificationExistsAsync(Guid notificationId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .AnyAsync(n => n.Id == notificationId && n.UserProfileId == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task MarkUserNotificationAsReadAsync(Guid notificationId, int userProfileId, CancellationToken cancellationToken = default)
        {
            await _context.Notifications
                .Where(n => n.Id == notificationId && n.UserProfileId == userProfileId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(n => n.IsRead, true), cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<int> MarkAllUserNotificationsAsReadAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.UserProfileId == userProfileId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(n => n.IsRead, true), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
