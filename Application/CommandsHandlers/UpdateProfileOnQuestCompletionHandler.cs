using Application.Common;
using Domain.Events;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.CommandsHandlers
{
    public class UpdateProfileOnQuestCompletionHandler
        : INotificationHandler<DomainEventNotification<QuestCompletedEvent>>,
        INotificationHandler<DomainEventNotification<QuestUncompletedEvent>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileOnQuestCompletionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DomainEventNotification<QuestCompletedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;
            var userProfile = await _unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.ApplyQuestCompletionRewards(notification.XpAwarded, notification.GoalsCompletedCount, notification.IsFirstTimeCompleted, notification.ShouldAssignRewards);
        }

        public async Task Handle(DomainEventNotification<QuestUncompletedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;
            var userProfile = await _unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.RevertQuestCompletion();
        }
    }
}
