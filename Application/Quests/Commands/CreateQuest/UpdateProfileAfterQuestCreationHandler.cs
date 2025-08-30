using Application.Common;
using Domain.Events.Quests;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Commands.CreateQuest
{
    public class UpdateProfileAfterQuestCreationHandler(IUnitOfWork unitOfWork)
        : INotificationHandler<DomainEventNotification<QuestCreatedEvent>>
    {
        public async Task Handle(DomainEventNotification<QuestCreatedEvent> wrappedNotification, CancellationToken cancellationToken = default)
        {
            var notification = wrappedNotification.DomainEvent;

            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(notification.UserProfileId, cancellationToken)
                ?? throw new NotFoundException($"Profile {notification.UserProfileId} not found.");

            userProfile.UpdateAfterQuestCreation();
        }
    }
}
