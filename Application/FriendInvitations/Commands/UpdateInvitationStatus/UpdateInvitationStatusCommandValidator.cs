using Domain.Interfaces;
using FluentValidation;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus
{
    public class UpdateInvitationStatusCommandValidator : AbstractValidator<UpdateInvitationStatusCommand>
    {
        public UpdateInvitationStatusCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.InvitationId)
                .GreaterThan(0).WithMessage("InvitationId must be greater than 0.")
                .MustAsync(async (command, invitationId, cancellationToken) =>
                {
                    return await unitOfWork.FriendInvitations.IsUserInvitationExistByIdAsync(command.UserProfileId, invitationId, cancellationToken).ConfigureAwait(false);
                }).WithMessage("Invitation doesn't exist or you don't have access to it.");
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");
            RuleFor(x => x.Status)
                .NotNull().WithMessage("Status is required.")
                .IsInEnum().WithMessage("Status must be a valid enum value. 'Accepted', 'Rejected' or 'Cancelled'.");
        }
    }
}
