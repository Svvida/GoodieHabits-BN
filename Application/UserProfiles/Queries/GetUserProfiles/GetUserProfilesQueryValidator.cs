using FluentValidation;

namespace Application.UserProfiles.Queries.GetUserProfiles
{
    public class GetUserProfilesQueryValidator : AbstractValidator<GetUserProfilesQuery>
    {
        public GetUserProfilesQueryValidator()
        {
            RuleFor(x => x.Nickname)
                .MaximumLength(50).WithMessage("Nickname search term cannot exceed 50 characters.");

            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Limit must be greater than 0.")
                .LessThanOrEqualTo(50).WithMessage("Maximum limit is 50.");
        }
    }
}
