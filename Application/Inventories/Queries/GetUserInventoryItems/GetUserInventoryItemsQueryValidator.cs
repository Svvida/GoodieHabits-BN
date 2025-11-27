using FluentValidation;

namespace Application.Inventories.Queries.GetUserInventoryItems
{
    public class GetUserInventoryItemsQueryValidator : AbstractValidator<GetUserInventoryItemsQuery>
    {
        public GetUserInventoryItemsQueryValidator()
        {
            RuleFor(x => x.UserProfileId).GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");
        }
    }
}
