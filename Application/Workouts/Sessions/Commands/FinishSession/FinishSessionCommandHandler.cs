using Application.Common;
using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.FinishSession
{
    /// <summary>
    /// Closes a session and raises <c>WorkoutSessionCompletedEvent</c> — the module's gamification hook.
    /// <para>
    /// Nothing consumes that event yet: awarding XP, coins or badges for training is a deliberate backlog item.
    /// The hook ships now so adding it later needs no migration and no pass over session history.
    /// </para>
    /// <para>
    /// Events are published before <c>SaveChangesAsync</c> and then cleared, matching
    /// <c>DeleteQuestCommandHandler</c>: there is no central dispatch in <c>SaveChanges</c> (ARCHITECTURE §9),
    /// and publishing first lets a future handler's own writes commit in the same unit of work.
    /// </para>
    /// </summary>
    public class FinishSessionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock, IPublisher publisher)
        : IRequestHandler<FinishSessionCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(FinishSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            session.Complete(clock.GetCurrentInstant().ToDateTimeUtc());

            foreach (var domainEvent in session.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            session.ClearDomainEvents();

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
