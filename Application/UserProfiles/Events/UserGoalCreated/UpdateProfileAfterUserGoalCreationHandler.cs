using Application.Common;
using Domain.Events.UserGoals;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.UserProfiles.Events.UserGoalCreated
{
    public class UpdateProfileAfterUserGoalCreationHandler(IUnitOfWork unitOfWork)
        : INotificationHandler<DomainEventNotification<UserGoalCreatedEvent>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(DomainEventNotification<UserGoalCreatedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;

            var userProfile = await _unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.UpdateAfterUserGoalCreation();
        }
    }
}
