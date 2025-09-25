using Application.Common;
using Domain.Events.Quests;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Commands.DeleteQuest
{
    internal class UpdateProfileAfterQuestDeletionHandler(IUnitOfWork unitOfWork) : INotificationHandler<DomainEventNotification<QuestDeletedEvent>>
    {
        public async Task Handle(DomainEventNotification<QuestDeletedEvent> wrapperNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrapperNotification.DomainEvent;
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(notification.UserProfileId, cancellationToken)
                ?? throw new NotFoundException($"Profile {notification.UserProfileId} not found.");

            var wasQuestActiveGoal = await unitOfWork.UserGoals.IsQuestActiveGoalAsync(notification.QuestId, cancellationToken)
                .ConfigureAwait(false);

            userProfile.UpdateAfterQuestDeletion(notification.IsQuestCompleted, notification.IsQuestEverCompleted, wasQuestActiveGoal);
        }
    }
}
