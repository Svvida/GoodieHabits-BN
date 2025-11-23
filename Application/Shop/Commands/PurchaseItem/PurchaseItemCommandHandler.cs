using Application.Statistics.Calculators;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Shop.Commands.PurchaseItem
{
    public class PurchaseItemCommandHandler(IUnitOfWork unitOfWork, IClock clock, ILevelCalculator levelCalculator) : IRequestHandler<PurchaseItemCommand, PurchaseItemResponse>
    {
        public async Task<PurchaseItemResponse> Handle(PurchaseItemCommand command, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(command.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {command.UserProfileId} not found.");

            var shopItem = await unitOfWork.ShopItems.GetByIdAsync(command.ShopItemId, cancellationToken).ConfigureAwait(false);

            if (shopItem is null || shopItem.IsPurchasable == false)
                throw new NotFoundException("Shop item not found or you can't buy it.");

            var userLevel = levelCalculator.CalculateLevelInfo(user.TotalXp).CurrentLevel;

            user.PurchaseItem(shopItem, userLevel, clock.GetCurrentInstant().ToDateTimeUtc());

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new PurchaseItemResponse(user.Coins);
        }
    }
}
