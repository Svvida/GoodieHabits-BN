using Application.Common;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Commands.DeleteQuest
{
    public class DeleteQuestCommandHandler : IRequestHandler<DeleteQuestCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public DeleteQuestCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Unit> Handle(DeleteQuestCommand command, CancellationToken cancellationToken)
        {
            var quest = await _unitOfWork.Quests.GetByIdAsync(command.QuestId, cancellationToken)
                ?? throw new NotFoundException($"Quest with ID {command.QuestId} not found.");

            quest.Delete();
            _unitOfWork.Quests.Remove(quest);

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await _publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            quest.ClearDomainEvents();

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
