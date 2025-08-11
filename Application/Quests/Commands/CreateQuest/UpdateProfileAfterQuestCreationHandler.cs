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

            var userProfile = await unitOfWork.UserProfiles.GetByAccountIdAsync(notification.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Profile for account {notification.AccountId} not found.");

            userProfile.UpdateAfterQuestCreation();
        }
    }
}
