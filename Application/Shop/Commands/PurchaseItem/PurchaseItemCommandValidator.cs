using Domain.Interfaces;
using FluentValidation;

namespace Application.Shop.Commands.PurchaseItem
{
    public class PurchaseItemCommandValidator : AbstractValidator<PurchaseItemCommand>
    {
        public PurchaseItemCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("UserProfileId must be greater than 0.")
                .MustAsync(unitOfWork.UserProfiles.ExistsByIdAsync).WithMessage($"User Profile not found.");

            RuleFor(x => x.ShopItemId)
                .GreaterThan(0).WithMessage("ShopItemId must be greated than 0.")
                .MustAsync(async (id, ct) =>
                {
                    var item = await unitOfWork.ShopItems.GetByIdAsync(id, ct);
                    return item != null && item.IsPurchasable;
                }).WithMessage("Shop item not found or unavailable.");
        }
    }
}
