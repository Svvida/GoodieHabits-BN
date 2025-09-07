using Domain.Interfaces;
using FluentValidation;

namespace Application.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
    {
        public MarkNotificationAsReadCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("UserProfileId must be greater than 0.");

            RuleFor(x => x.NotificationId)
                .MustAsync(async (command, notificationId, cancellationToken) =>
                {
                    return await unitOfWork.Notifications.IsUserNotificationExistsAsync(notificationId, command.UserProfileId, cancellationToken).ConfigureAwait(false);
                }).WithMessage("Notification with the given ID does not exist.");
        }
    }
}
