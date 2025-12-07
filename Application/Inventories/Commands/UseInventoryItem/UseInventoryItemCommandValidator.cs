using FluentValidation;

namespace Application.Inventories.Commands.UseInventoryItem
{
    public class UseInventoryItemCommandValidator : AbstractValidator<UseInventoryItemCommand>
    {
        public UseInventoryItemCommandValidator()
        {
            RuleFor(x => x.UserProfileId).GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");
            RuleFor(x => x.InventoryId).GreaterThan(0).WithMessage("InventoryId must be greater than 0.");
        }
    }
}
