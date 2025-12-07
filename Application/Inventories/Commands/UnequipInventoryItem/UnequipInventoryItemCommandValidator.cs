using FluentValidation;

namespace Application.Inventories.Commands.UnequipInventoryItem
{
    public class UnequipInventoryItemCommandValidator : AbstractValidator<UnequipInventoryItemCommand>
    {
        public UnequipInventoryItemCommandValidator()
        {
            RuleFor(x => x.UserProfileId).GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");
            RuleFor(x => x.InventoryId).GreaterThan(0).WithMessage("InventoryId must be greater than 0.");
        }
    }
}
