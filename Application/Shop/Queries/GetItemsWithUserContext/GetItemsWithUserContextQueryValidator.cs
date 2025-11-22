using FluentValidation;

namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public class GetItemsWithUserContextQueryValidator : AbstractValidator<GetItemsWithUserContextQuery>
    {
        public GetItemsWithUserContextQueryValidator()
        {
            RuleFor(query => query.UserProfileId).GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");
            RuleFor(query => query.SortBy).IsInEnum().WithMessage("SortBy is not a valid enumeration value.");
            RuleFor(query => query.SortOrder).IsInEnum().WithMessage("SortOrder is not a valid enumeration value.");
        }
    }
}
