using FluentValidation;

namespace Application.FriendInvitations.Queries.GetUserInvitations
{
    public class GetFriendInvitationsQueryValidator : AbstractValidator<GetUserInvitationsQuery>
    {
        public GetFriendInvitationsQueryValidator()
        {
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");

            RuleFor(x => x.Direction)
                .IsInEnum().When(x => x.Direction.HasValue)
                .WithMessage("Direction must be a valid value.");
        }
    }
}
