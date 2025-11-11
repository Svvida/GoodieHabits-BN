using Domain.Interfaces;
using FluentValidation;

namespace Application.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public class GetUserProfileForPublicDisplayQueryValidator : AbstractValidator<GetUserProfileForPublicDisplayQuery>
    {
        public GetUserProfileForPublicDisplayQueryValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.CurrentUserProfileId)
                .GreaterThan(0).WithMessage("CurrentUserProfileId must be greater than 0.");
            RuleFor(x => x.ViewedUserProfileId)
                .GreaterThan(0).WithMessage("ViewedUserProfileId must be greater than 0.")
                .NotEqual(x => x.CurrentUserProfileId).WithMessage("Viewed User Profile Id must not equal Current User Profile Id.")
                .MustAsync(async (query, value, cancellationToken) =>
                {
                    return await unitOfWork.UserProfiles.ExistsByIdAsync(value, cancellationToken).ConfigureAwait(false);
                }).WithMessage("Viewed User Profile Id must exists.");
        }
    }
}
