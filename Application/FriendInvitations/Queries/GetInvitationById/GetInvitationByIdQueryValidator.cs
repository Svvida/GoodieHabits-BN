using Domain.Interfaces;
using FluentValidation;

namespace Application.FriendInvitations.Queries.GetInvitationById
{
    public class GetInvitationByIdQueryValidator : AbstractValidator<GetInvitationByIdQuery>
    {
        public GetInvitationByIdQueryValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("User profile ID must be greater than zero.");
            RuleFor(x => x.InvitationId)
                .GreaterThan(0).WithMessage("Invitation ID must be greater than zero.")
                .MustAsync(async (query, invitationId, cancellationToken) =>
                {
                    return await unitOfWork.FriendInvitations.IsUserInvitationExistByIdAsync(query.UserProfileId, invitationId, cancellationToken).ConfigureAwait(false);
                }).WithMessage("Invitation not found, or you don't have access to it.");
        }
    }
}
