using Application.Common.Interfaces.Notifications;
using Application.Common.Notifications;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Shop.EventHandlers
{
    public class AwardLevelUpItemsHandler(IUnitOfWork unitOfWork, IClock clock, INotificationService notificationService) : INotificationHandler<UserLeveledUpNotification>
    {
        public async Task Handle(UserLeveledUpNotification notification, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(notification.UserProfileId, false, cancellationToken).ConfigureAwait(false);
            if (userProfile is null)
            {
                return;
            }

            var levelUpItems = await unitOfWork.ShopItems.GetFreeItemsUnlockableAtLevelAsync(notification.NewLevel, cancellationToken).ConfigureAwait(false);
            if (levelUpItems is null || !levelUpItems.Any())
            {
                return;
            }
            foreach (var item in levelUpItems)
            {
                if (item.IsUnique && userProfile.InventoryItems.Any(ii => ii.ShopItemId == item.Id))
                    continue;

                userProfile.GrandItem(item, clock.GetCurrentInstant().ToDateTimeUtc());

                var payloadData = new
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    ItemImageUrl = item.ImageUrl
                };

                await notificationService.CreateAndSendAsync(
                    notification.UserProfileId,
                    NotificationTypeEnum.ShopItemAwarded,
                    "New Item Awarded!",
                    $"Congratulations! You've been awarded a new item: {item.Name} for reaching level {notification.NewLevel}. Check your inventory to see your new item!",
                    payloadData,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
