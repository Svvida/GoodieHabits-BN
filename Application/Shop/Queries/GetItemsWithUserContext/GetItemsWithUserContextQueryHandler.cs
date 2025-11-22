using Application.Common.Sorting;
using Application.Statistics.Calculators;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public class GetItemsWithUserContextQueryHandler(IUnitOfWork unitOfWork, ILevelCalculator levelCalculator, IMapper mapper) : IRequestHandler<GetItemsWithUserContextQuery, List<ShopItemDto>>
    {
        public async Task<List<ShopItemDto>> Handle(GetItemsWithUserContextQuery request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(request.UserProfileId, cancellationToken)
                ?? throw new NotFoundException($"User with ID {request.UserProfileId} not found.");

            var query = unitOfWork.ShopItems.GetAvailableItemsQuery(request.Category);

            query = request.SortBy switch
            {
                ShopItemSortProperty.Id => query.ApplyOrder(x => x.Id, request.SortOrder),
                ShopItemSortProperty.Price => query.ApplyOrder(x => x.Price, request.SortOrder),
                ShopItemSortProperty.Name => query.ApplyOrder(x => x.Name, request.SortOrder),
                ShopItemSortProperty.LevelRequirement => query.ApplyOrder(x => x.LevelRequirement, request.SortOrder),
                _ => query.ApplyOrder(x => x.Id, request.SortOrder),
            };

            var items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            var userLevel = levelCalculator.CalculateLevelInfo(user.TotalXp).CurrentLevel;

            // Efficient lookup for Inventory
            var inventoryLookup = user.InventoryItems.ToDictionary(ii => ii.ShopItemId, ii => ii.Quantity);

            List<ShopItemDto> response = new(items.Count); // Pre-size the list

            foreach (var item in items)
            {
                var quantityOwned = inventoryLookup.GetValueOrDefault(item.Id, 0);
                bool isOwnedByUser = quantityOwned > 0;

                var isUnlocked = item.LevelRequirement <= userLevel;
                var canAfford = user.Coins >= item.Price;

                PurchaseLockReasonEnum? purchaseLockReason;

                if (isOwnedByUser && item.IsUnique)
                    purchaseLockReason = PurchaseLockReasonEnum.AlreadyOwned;
                else if (!isUnlocked)
                    purchaseLockReason = PurchaseLockReasonEnum.InsufficientLevel;
                else if (!canAfford)
                    purchaseLockReason = PurchaseLockReasonEnum.InsufficientFunds;
                else
                    purchaseLockReason = null;

                var canPurchase = purchaseLockReason == null;

                var userContext = new UserContextDto(
                    isOwnedByUser,
                    quantityOwned,
                    isUnlocked, // item.LevelRequirement <= userLevel
                    canAfford, // user.Coins >= Price
                    canPurchase, // final determination if user can purchase
                    purchaseLockReason?.ToString());

                var itemDto = mapper.Map<ShopItemDto>(item) with { UserContext = userContext };
                response.Add(itemDto);
            }

            return response;
        }
    }
}
