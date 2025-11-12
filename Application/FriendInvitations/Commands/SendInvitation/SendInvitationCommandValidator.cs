using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using NodaTime;

namespace Application.FriendInvitations.Commands.SendInvitation
{
    public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        public SendInvitationCommandValidator(IUnitOfWork unitOfWork, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;

            RuleFor(x => x.SenderUserProfileId)
                .GreaterThan(0).WithMessage("Sender user profile ID must be greater than zero.");

            RuleFor(x => x.ReceiverUserProfileId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Receiver user profile ID must be greater than zero.")
                .NotEqual(x => x.SenderUserProfileId).WithMessage("Sender and Receiver cannot be the same user.")
                .MustAsync(async (command, receiverUserProfileId, cancellationToken) =>
                {
                    return await _unitOfWork.UserProfiles
                        .ExistsByIdAsync(receiverUserProfileId, cancellationToken).ConfigureAwait(false);
                }).WithMessage("Receiver user profile not found.")
                .CustomAsync(BeEligibleForFriendship);
        }

        private async Task BeEligibleForFriendship(
            int receiverUserProfileId,
            ValidationContext<SendInvitationCommand> context,
            CancellationToken cancellationToken)
        {
            var command = context.InstanceToValidate;

            var status = await _unitOfWork.FriendInvitations.CheckFriendshipEligibilityAsync(
                command.SenderUserProfileId,
                receiverUserProfileId, _clock.GetCurrentInstant(), cancellationToken).ConfigureAwait(false);

            if (status != FriendshipEligibilityStatus.Eligible)
            {
                context.AddFailure(
                    context.PropertyPath,
                    $"Unable to send invitation. Reason: {status}.");
            }
        }
    }
}
