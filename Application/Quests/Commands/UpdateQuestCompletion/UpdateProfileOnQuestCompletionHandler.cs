using Application.Common;
using Domain.Events.Quests;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Commands.UpdateQuestCompletion
{
    public class UpdateProfileOnQuestCompletionHandler(IUnitOfWork unitOfWork)
                : INotificationHandler<DomainEventNotification<QuestCompletedEvent>>,
        INotificationHandler<DomainEventNotification<QuestUncompletedEvent>>
    {
        public async Task Handle(DomainEventNotification<QuestCompletedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;
            var userProfile = await unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.ApplyQuestCompletionRewards(notification.XpAwarded, notification.IsGoalCompleted, notification.IsFirstTimeCompleted, notification.ShouldAssignRewards);
        }

        public async Task Handle(DomainEventNotification<QuestUncompletedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;
            var userProfile = await unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.RevertQuestCompletion();
        }
    }
}
