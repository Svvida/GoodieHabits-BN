using Application.Common;
using Domain.Events.UserGoals;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.UserGoals.Commands.CreateUserGoal
{
    public class UpdateProfileAfterUserGoalCreationHandler(IUnitOfWork unitOfWork)
        : INotificationHandler<DomainEventNotification<UserGoalCreatedEvent>>
    {
        public async Task Handle(DomainEventNotification<UserGoalCreatedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;

            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(notification.UserProfileId, cancellationToken)
                ?? throw new NotFoundException($"Profile {notification.UserProfileId} not found.");

            userProfile.UpdateAfterUserGoalCreation();
        }
    }
}
