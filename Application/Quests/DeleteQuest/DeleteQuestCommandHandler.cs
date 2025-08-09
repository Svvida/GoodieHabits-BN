using Application.Common;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.DeleteQuest
{
    public class DeleteQuestCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher) : IRequestHandler<DeleteQuestCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteQuestCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetByIdAsync(command.QuestId, cancellationToken)
                ?? throw new NotFoundException($"Quest with ID {command.QuestId} not found.");

            quest.Delete();
            unitOfWork.Quests.Remove(quest);

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            quest.ClearDomainEvents();

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
