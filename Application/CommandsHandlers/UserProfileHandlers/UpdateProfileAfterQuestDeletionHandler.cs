using Application.Common;
using Domain.Events;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.CommandsHandlers.UserProfileHandlers
{
    internal class UpdateProfileAfterQuestDeletionHandler : INotificationHandler<DomainEventNotification<QuestDeletedEvent>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileAfterQuestDeletionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DomainEventNotification<QuestDeletedEvent> wrapperNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrapperNotification.DomainEvent;
            var userProfile = await _unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            var wasQuestActiveGoal = await _unitOfWork.UserGoals.IsQuestActiveGoalAsync(notification.QuestId, cancellationToken)
                .ConfigureAwait(false);

            userProfile.UpdateAfterQuestDeletion(notification.IsQuestCompleted, notification.IsQuestEverCompleted, wasQuestActiveGoal);
        }
    }
}
